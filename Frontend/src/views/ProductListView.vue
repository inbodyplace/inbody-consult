<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faCircleInfo, faPlus } from '@fortawesome/free-solid-svg-icons'
import ProductCard from '../components/molecules/product-list/ProductCard.vue'
import ProductAddDialog from '../components/organisms/product-list/ProductAddDialog.vue'
import { createProduct, loadProducts, products } from '../stores/products.js'

const router = useRouter()
const isAddDialogOpen = ref(false)
const isLoading = ref(false)
const isSaving = ref(false)
const pageError = ref('')
const saveError = ref('')
const memberCount = computed(() => products.reduce((sum, product) => sum + product.included, 0))

onMounted(async () => {
  isLoading.value = true
  pageError.value = ''
  try {
    await loadProducts()
  } catch (error) {
    pageError.value = error.message
  } finally {
    isLoading.value = false
  }
})

async function addProduct(name) {
  isSaving.value = true
  saveError.value = ''
  try {
    const product = await createProduct(name)
    isAddDialogOpen.value = false
    router.push({ name: 'product-detail', params: { id: product.id } })
  } catch (error) {
    saveError.value = error.message
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <main class="page-content">
    <div class="page-content__inner">
      <header class="page-header">
        <div>
          <h1>Programs</h1>
          <p class="page-header__intro">
            Record each member's initial and follow-up measurements for every program. Compare
            skeletal muscle mass, body fat mass, weight, and body fat percentage. Only complete
            measurement pairs contribute to the average.
          </p>
          <p class="page-header__summary">
            <FontAwesomeIcon :icon="faCircleInfo" aria-hidden="true" />
            {{ products.length }} programs · {{ memberCount }} members recorded
          </p>
          <p v-if="pageError" class="page-message" role="alert">{{ pageError }}</p>
          <p v-else-if="isLoading" class="page-message">Loading programs...</p>
        </div>
        <button class="page-header__add" type="button" @click="isAddDialogOpen = true">
          <FontAwesomeIcon :icon="faPlus" aria-hidden="true" />
          Add program
        </button>
      </header>

      <section class="product-list" aria-label="Programs" tabindex="0">
        <div class="product-list__grid">
          <ProductCard
            v-for="product in products"
            :key="product.id"
            :product="product"
            @show-details="router.push({ name: 'product-detail', params: { id: $event } })"
            @use-consultation="router.push({ name: 'simulator', query: { productId: $event } })"
          />
        </div>
      </section>
    </div>
    <ProductAddDialog
      v-if="isAddDialogOpen"
      :loading="isSaving"
      :error="saveError"
      @cancel="isAddDialogOpen = false"
      @add="addProduct"
    />
  </main>
</template>

<style scoped lang="scss">
.page-content {
  height: 100%;
  min-height: 0;
  overflow: hidden;
  padding: 32px 24px 16px;

  &__inner {
    display: flex;
    width: min(100%, 1120px);
    height: 100%;
    margin: 0 auto;
    flex-direction: column;
    min-height: 0;
  }
}

.page-header {
  display: flex;
  flex: none;
  margin-bottom: 16px;
  align-items: center;
  justify-content: space-between;
  gap: 32px;

  > div {
    min-width: 0;
  }

  h1 {
    margin: 0 0 8px;
    color: var(--color-text);
    font-size: clamp(24px, 2vw, 32px);
    letter-spacing: -0.04em;
  }

  &__intro {
    max-width: 720px;
    margin: 0;
    color: #526171;
    font-size: 14px;
    line-height: 1.6;
  }

  &__summary {
    margin: 8px 0 0;
    color: #75859a;
    font-size: 13px;
  }

  &__add {
    flex: none;
    padding: 14px 22px;
    border: 0;
    border-radius: 12px;
    background: var(--color-primary);
    color: #fff;
    font: inherit;
    font-size: 14px;
    font-weight: 700;
    cursor: pointer;
  }
}

.page-message {
  margin: 8px 0 0;
  color: #bd4545;
  font-size: 13px;
}

.product-list {
  min-height: 0;
  flex: 1;
  overflow-y: auto;
  padding: 20px 4px 1px 1px;

  &__grid {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 16px;
  }
}

@media (max-width: 1000px) {
  .product-list__grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 620px) {
  .page-content {
    padding: 20px 16px 20px;
  }

  .page-header {
    align-items: flex-start;
    flex-direction: column;
    gap: 16px;

    &__add {
      align-self: flex-end;
    }
  }

  .product-list {
    &__grid {
      grid-template-columns: 1fr;
    }
  }
}

</style>
