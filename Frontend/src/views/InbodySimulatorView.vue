<script setup>
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import ConsultationPrep from '../components/organisms/inbody-simulator/ConsultationPrep.vue'
import ConsultationResult from '../components/organisms/inbody-simulator/ConsultationResult.vue'
import SimilarMembers from '../components/organisms/inbody-simulator/SimilarMembers.vue'
import { api } from '../api/client.js'
import { loadProducts, products as productStore } from '../stores/products.js'
import { mapGender } from '../utils/measurements.js'

const route = useRoute()
const products = ref([])
const productsLoading = ref(false)
const productsError = ref('')
const result = ref(null)
const similarMembers = ref([])
const isSearching = ref(false)
const searchError = ref('')

// Load programs and convert them to selection options on entry.
onMounted(async () => {
  productsLoading.value = true
  productsError.value = ''
  try {
    await loadProducts()
    products.value = productStore.map((product) => ({ value: String(product.id), label: product.name }))
  } catch (error) {
    productsError.value = error.message
  } finally {
    productsLoading.value = false
  }
})

// Fetch similar members and program statistics, then calculate projected changes.
async function showPreview(input) {
  isSearching.value = true
  searchError.value = ''
  result.value = null
  similarMembers.value = []
  try {
    // Convert input to the API's gender and measurement format.
    const gender = input.gender === 'female' ? 'F' : 'M'
    const current = {
      smm: input.current.muscleMass,
      bfm: input.current.bodyFatMass,
      weight: input.current.weight,
      pbf: input.current.bodyFatPercentage,
    }
    const [similar, product] = await Promise.all([
      api.post(`/api/products/${input.productId}/similar-members/search`, { gender, current }),
      api.get(`/api/products/${input.productId}`),
    ])
    // Apply monthly averages only with three complete records for the same gender.
    const genderStats = product.stats?.[gender]
    const average = genderStats?.monthlyAverageChange
    const projected = average && genderStats.completedCount >= 3
      ? {
          muscleMass: current.smm + average.smm * input.period,
          bodyFatMass: current.bfm + average.bfm * input.period,
          weight: current.weight + average.weight * input.period,
          bodyFatPercentage: current.pbf + average.pbf * input.period,
        }
      : null

    result.value = {
      ...input,
      projected,
      completedCount: genderStats?.completedCount ?? 0,
    }
    // Convert similar-member responses to the component format.
    similarMembers.value = similar.map((member, index) => ({
      id: `similar-${index + 1}`,
      name: `Member ${index + 1}`,
      gender: mapGender(member.gender),
      startDate: member.startDate?.replaceAll('-', '.'),
      endDate: member.endDate?.replaceAll('-', '.'),
      before: { muscleMass: member.before.smm, bodyFatMass: member.before.bfm, weight: member.before.weight, bodyFatPercentage: member.before.pbf },
      after: { muscleMass: member.after.smm, bodyFatMass: member.after.bfm, weight: member.after.weight, bodyFatPercentage: member.after.pbf },
    }))
  } catch (error) {
    searchError.value = error.message
  } finally {
    isSearching.value = false
  }
}
</script>

<template>
  <main class="page-content">
    <div class="page-content__inner">
      <header class="page-header">
        <h1>Body Composition Forecast</h1>
        <p>
          Enter current InBody measurements and a program to explore projected changes over time.
          You can also review members with similar starting measurements.
        </p>
      </header>
      <div class="simulator-layout">
        <ConsultationPrep
          :products="products"
          :products-loading="productsLoading"
          :products-error="productsError"
          :searching="isSearching"
          :initial-product-id="route.query.productId"
          @preview="showPreview"
        />
        <div class="results-column">
          <p v-if="searchError" class="search-error" role="alert">{{ searchError }}</p>
          <ConsultationResult :result="result" />
          <SimilarMembers v-if="result" :members="similarMembers" :gender="result.gender" />
        </div>
      </div>
    </div>
  </main>
</template>

<style scoped lang="scss">
.page-content {
  padding: 40px 24px 72px;
  min-height: 100%;

  &__inner {
    width: min(100%, 1120px);
    margin: 0 auto;
  }
}

.simulator-layout {
  display: grid;
  grid-template-columns: minmax(0, 400px) minmax(0, 1fr);
  align-items: start;
  gap: 20px;
}

.results-column {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 20px;
}

.search-error {
  grid-column: 1 / -1;
  margin: 0;
  color: #c0392b;
  font-size: 13px;
}

.page-header {
  margin-bottom: 24px;

  h1 {
    margin: 0 0 8px;
    color: var(--color-text);
    font-size: clamp(22px, 2vw, 28px);
    letter-spacing: -0.04em;
  }

  p {
    max-width: 720px;
    margin: 0;
    color: #526171;
    font-size: 14px;
    line-height: 1.6;
  }
}

@media (max-width: 900px) {
  .simulator-layout {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 600px) {
  .page-content {
    padding: 26px 16px 48px;
  }
}
</style>
