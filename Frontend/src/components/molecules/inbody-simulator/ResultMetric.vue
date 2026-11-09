<script setup>
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faArrowRight } from '@fortawesome/free-solid-svg-icons'
defineProps({
  label: { type: String, required: true },
  unit: { type: String, required: true },
  current: { type: Number, required: true },
  projected: { type: Number, default: null },
  period: { type: Number, required: true },
})
</script>

<template>
  <article class="metric">
    <div class="metric__heading">
      <h3>{{ label }}</h3>
      <span>{{ unit }}</span>
    </div>
    <div class="metric__values">
      <div class="metric__current">
        <span>Current</span><strong>{{ current.toFixed(1) }}</strong>
      </div>
      <FontAwesomeIcon v-if="projected !== null" class="metric__arrow" :icon="faArrowRight" aria-hidden="true" />
      <div class="metric__projected">
        <span>After {{ period }} months</span>
        <strong v-if="projected !== null">{{ projected.toFixed(1) }}<small>{{ unit }}</small></strong>
        <strong v-else class="metric__unavailable">Unavailable</strong>
      </div>
    </div>
  </article>
</template>

<style scoped lang="scss">
.metric {
  min-width: 0;
  min-height: 128px;
  padding: 16px;
  border: 1px solid #dfe7f1;
  border-radius: 13px;
  background: #fff;

  &__heading,
  &__values {
    display: flex;
    justify-content: space-between;
  }

  &__heading {
    align-items: center;
    gap: 8px;
  }

  &__heading h3 {
    min-width: 0;
    margin: 0;
    font-size: 14px;
    line-height: 1.35;
  }

  &__heading > span {
    flex: none;
    color: #75859a;
    font-size: 12px;
  }

  &__values {
    align-items: end;
    margin-top: 15px;
    gap: 8px;
  }

  &__current,
  &__projected {
    display: flex;
    min-width: 0;
    flex-direction: column;
    gap: 3px;
  }

  &__current > span,
  &__projected > span {
    color: #75859a;
    font-size: 11px;
  }

  &__current strong {
    color: #526171;
    font-size: 15px;
  }

  &__projected {
    align-items: flex-end;
    text-align: right;
  }

  &__projected > span,
  &__projected strong {
    color: var(--color-primary);
  }

  &__projected strong.metric__unavailable {
    color: #75859a;
    font-size: 14px;
  }

  &__projected strong {
    font-size: 26px;
    line-height: 1;
  }

  &__projected small {
    margin-left: 2px;
    font-size: 11px;
    font-weight: 500;
  }

  &__arrow {
    padding-bottom: 4px;
    color: #9aabba;
    font-size: 17px;
  }
}
</style>
