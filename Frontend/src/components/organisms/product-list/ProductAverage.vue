<script setup>
import { computed } from 'vue'
const props = defineProps({ product: { type: Object, required: true } })
const metrics = [
  { key: 'smm', label: 'Skeletal muscle mass', unit: 'kg' },
  { key: 'bfm', label: 'Body fat mass', unit: 'kg' },
  { key: 'weight', label: 'Weight', unit: 'kg' },
  { key: 'pbf', label: 'Body fat percentage', unit: '%' },
]
const statistics = computed(() => props.product.stats)
const completedCount = computed(() => statistics.value?.overall?.completedCount ?? props.product.before)
const maleCount = computed(() => statistics.value?.M?.completedCount ?? 0)
const femaleCount = computed(() => statistics.value?.F?.completedCount ?? 0)
function averageChange(metric) {
  const value = statistics.value?.overall?.monthlyAverageChange?.[metric.key]
    ?? props.product.monthlyAverageChange?.[metric.key]
  if (!Number.isFinite(value)) return '–'
  return `${value > 0 ? '+' : ''}${value.toFixed(2)} ${metric.unit}`
}
</script>

<template>
  <section class="average-panel" aria-labelledby="average-title">
    <div class="average-panel__header">
      <h2 id="average-title">Average monthly change by metric</h2>
      <span
        >Complete pairs: {{ completedCount }} (male: {{ maleCount }} · female:
        {{ femaleCount }})</span
      >
    </div>
    <div class="average-panel__grid">
      <div v-for="metric in metrics" :key="metric.key">
        <span>{{ metric.label }}</span>
        <strong>{{ averageChange(metric) }}</strong>
      </div>
    </div>
  </section>
</template>

<style scoped lang="scss">
.average-panel {
  border: 1px solid #dfe7f1;
  border-radius: 15px;
  background: #fff;
  padding: 16px;
  margin-bottom: 16px;

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    flex-wrap: wrap;
    margin-bottom: 12px;

    h2 {
      margin: 0;
      font-size: 16px;
      line-height: 1.35;
    }

    span {
      color: #75859a;
      font-size: 13px;
      line-height: 1.4;
    }
  }

  &__grid {
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 7px;

    > div {
      min-width: 0;
      min-height: 63px;
      padding: 12px;
      border-radius: 12px;
      background: #f6f9fd;
    }

    span,
    strong {
      display: block;
    }

    span {
      color: #75859a;
      font-size: 13px;
      line-height: 1.35;
    }

    strong {
      margin-top: 7px;
      color: var(--color-primary);
      font-size: 16px;
    }
  }
}

@media (max-width: 700px) {
  .average-panel__grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
