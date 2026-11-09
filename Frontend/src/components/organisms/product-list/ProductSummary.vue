<script setup>
import { computed } from 'vue'
const props = defineProps({ product: { type: Object, required: true } })
const registeredCount = computed(() => props.product.included)
const completedCount = computed(() => props.product.stats?.overall?.completedCount ?? props.product.before)
const excludedCount = computed(() => registeredCount.value - completedCount.value)
</script>

<template>
  <section class="summary-grid" aria-label="Program overview">
    <div>
      <span>Registered members</span><strong>{{ registeredCount }}<small> members</small></strong>
    </div>
    <div>
      <span>Complete measurement pairs</span><strong>{{ completedCount }}<small> members</small></strong>
    </div>
    <div>
      <span>Excluded from average</span><strong>{{ excludedCount }}<small> members</small></strong>
    </div>
  </section>
</template>

<style scoped lang="scss">
.summary-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
  margin-bottom: 12px;

  > div {
    padding: 16px;
    border: 1px solid #dfe7f1;
    border-radius: 15px;
    background: #fff;
  }

  span {
    display: block;
    color: #75859a;
    font-size: 13px;
  }

  strong {
    display: block;
    margin-top: 7px;
    color: var(--color-text);
    font-size: 24px;
  }

  small {
    font-size: 13px;
    font-weight: 400;
  }
}

@media (max-width: 700px) {
  .summary-grid {
    grid-template-columns: 1fr;
  }
}
</style>
