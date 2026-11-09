<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faChevronRight, faFileLines } from '@fortawesome/free-solid-svg-icons'
import ProductSummary from '../components/organisms/product-list/ProductSummary.vue'
import ProductAverage from '../components/organisms/product-list/ProductAverage.vue'
import ProductMembers from '../components/organisms/product-list/ProductMembers.vue'
import { loadProductDetail, loadProductMembers, loadProducts, products } from '../stores/products.js'

const route = useRoute()
const router = useRouter()
const isLoading = ref(true)
const loadError = ref('')
const product = computed(() => products.find((item) => item.id === Number(route.params.id)))

async function loadProduct() {
  isLoading.value = true
  loadError.value = ''
  try {
    if (!products.length) await loadProducts()
    if (!product.value) return
    await Promise.all([
      loadProductDetail(product.value.id),
      loadProductMembers(product.value.id),
    ])
  } catch (error) {
    loadError.value = error.message
  } finally {
    isLoading.value = false
  }
}

onMounted(loadProduct)
watch(() => route.params.id, loadProduct)
</script>

<template>
  <main v-if="isLoading" class="detail-page detail-page--missing">
    <p>Loading program details...</p>
  </main>
  <main v-else-if="loadError" class="detail-page detail-page--missing">
    <p role="alert">{{ loadError }}</p>
    <button type="button" @click="loadProduct">Try again</button>
  </main>
  <main v-else-if="product" class="detail-page">
    <div class="detail-page__inner">
      <nav class="detail-breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'products' }">Programs</RouterLink>
        <FontAwesomeIcon :icon="faChevronRight" aria-hidden="true" />
        <span aria-current="page">{{ product.name }}</span>
      </nav>
      <header class="detail-header">
        <div>
          <div class="detail-header__meta">
            <span>Created {{ product.registeredAt }}</span>
          </div>
          <h1>{{ product.name }}</h1>
          <p>
            Track all four measurements at the start and about a month later. Only members with
            both measurements contribute to the average.
          </p>
        </div>
        <button
          class="detail-header__consult"
          type="button"
          @click="router.push({ name: 'simulator', query: { productId: product.id } })"
        >
          <FontAwesomeIcon :icon="faFileLines" aria-hidden="true" />
          Use in consultation
        </button>
      </header>
      <ProductSummary :product="product" />
      <ProductAverage :product="product" />
      <ProductMembers :product="product" />
    </div>
  </main>
  <main v-else class="detail-page detail-page--missing">
    <p>Program not found.</p>
    <button type="button" @click="router.push({ name: 'products' })">Back to programs</button>
  </main>
</template>

<style scoped lang="scss">
.detail-page {
  min-height: 100%;
  padding: 24px 24px 32px;

  &__inner {
    width: min(100%, 1120px);
    margin: 0 auto;
  }

  &--missing {
    padding-top: 64px;
    text-align: center;
  }
}

.detail-breadcrumb {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 18px;
  color: #75859a;
  font-size: 13px;

  a {
    color: var(--color-primary);
    font-weight: 700;
    text-decoration: none;

    &:hover {
      text-decoration: underline;
    }

    &:focus-visible {
      outline: 2px solid var(--color-primary);
      outline-offset: 3px;
    }
  }

  svg {
    width: 9px;
    height: 9px;
  }
}

.detail-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 24px;
  margin-bottom: 24px;

  > div {
    min-width: 0;
  }

  &__meta {
    display: flex;
    align-items: center;
    gap: 10px;
    color: #75859a;
    font-size: 13px;
  }

  h1 {
    margin: 15px 0 8px;
    color: var(--color-text);
    font-size: clamp(26px, 2.3vw, 32px);
    letter-spacing: -0.04em;
    overflow-wrap: anywhere;
  }

  p {
    margin: 0;
    color: #526171;
    font-size: 15px;
    line-height: 1.6;
  }

  &__consult {
    flex: none;
    margin-top: 2px;
    padding: 12px 17px;
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

@media (max-width: 700px) {
  .detail-page {
    padding: 28px 16px 32px;
  }

  .detail-header {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
