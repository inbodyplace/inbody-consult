<script setup>
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faBullseye, faFileLines } from '@fortawesome/free-solid-svg-icons'
defineProps({
  product: { type: Object, required: true },
})
const emit = defineEmits(['show-details', 'use-consultation'])

const metrics = [
  { key: 'muscle', label: 'Skeletal muscle mass', unit: 'kg' },
  { key: 'fat', label: 'Body fat mass', unit: 'kg' },
  { key: 'weight', label: 'Weight', unit: 'kg' },
  { key: 'percentage', label: 'Body fat percentage', unit: '%' },
]
</script>

<template>
  <article class="product-card">
    <div class="product-card__top">
      <span class="product-card__badge">
        <FontAwesomeIcon :icon="faBullseye" aria-hidden="true" />
        Compare four metrics
      </span>
    </div>

    <h2>{{ product.name }}</h2>

    <div class="product-card__metrics">
      <p>Average monthly change by metric</p>
      <div class="product-card__metrics-grid">
        <div v-for="metric in metrics" :key="metric.key" class="product-card__metric">
          <span>{{ metric.label }}</span>
          <strong>
            {{ product[metric.key] }}
            <small v-if="product[metric.key] !== '—'">{{ metric.unit }}</small>
          </strong>
        </div>
      </div>
    </div>

    <p class="product-card__count">
      Complete pairs: {{ product.before }} · Registered: {{ product.included }} · Excluded:
      {{ product.excluded }}
    </p>

    <div class="product-card__actions">
      <button class="product-card__use" type="button" @click="emit('use-consultation', product.id)">
        <FontAwesomeIcon :icon="faFileLines" aria-hidden="true" />
        Use in consultation
      </button>
      <button class="product-card__detail" type="button" @click="emit('show-details', product.id)">
        Details
      </button>
    </div>
  </article>
</template>

<style scoped lang="scss">
.product-card {
  min-width: 0;
  padding: 20px;
  border: 1px solid #dfe7f1;
  border-radius: 18px;
  background: #fff;

  &__top {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  &__badge {
    padding: 6px 10px;
    border-radius: 20px;
    background: #edf4fb;
    color: var(--color-primary);
    font-size: 11px;
    font-weight: 700;
  }

  h2 {
    margin: 10px 0;
    color: var(--color-text);
    font-size: 18px;
    line-height: 1.3;
    overflow-wrap: anywhere;
  }

  &__metrics {
    padding: 12px 16px;
    border-radius: 13px;
    background: #f6f9fd;

    p {
      margin: 0 0 5px;
      color: #75859a;
      font-size: 12px;
    }
  }

  &__metrics-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;

    span {
      color: #526171;
      font-size: 12px;
      line-height: 1.35;
    }

    strong {
      color: var(--color-primary);
      font-size: 14px;
      white-space: nowrap;
    }

    small {
      color: #75859a;
      font-size: 11px;
      font-weight: 400;
    }
  }

  &__metric {
    display: flex;
    min-width: 0;
    flex-direction: column;
    justify-content: space-between;
    gap: 4px;
  }

  &__count {
    margin: 10px 0 16px;
    color: #75859a;
    font-size: 12px;
  }

  &__actions {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto;
    gap: 10px;
  }

  &__use,
  &__detail {
    min-height: 40px;
    padding: 8px 12px;
    border-radius: 12px;
    font: inherit;
    font-size: 14px;
    cursor: pointer;
  }

  &__use {
    border: 0;
    background: var(--color-primary);
    color: #fff;
    font-weight: 700;
  }

  &__detail {
    padding: 0 18px;
    border: 1px solid #dfe7f1;
    background: #fff;
    color: var(--color-text);
  }

  button:focus-visible {
    outline: 2px solid var(--color-primary);
    outline-offset: 3px;
  }
}

@media (max-width: 620px) {
  .product-card__count {
    line-height: 1.5;
  }
}
</style>
