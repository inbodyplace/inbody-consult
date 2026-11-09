<script setup>
defineProps({
  selected: { type: Boolean, default: false },
})
</script>

<template>
  <button class="choice-button" :class="{ 'choice-button--selected': selected }" type="button" :aria-pressed="selected">
    <slot />
  </button>
</template>

<style scoped lang="scss">
.choice-button {
  min-height: 38px;
  padding: 7px 14px;
  border: 1px solid #d8e2ef;
  border-radius: 18px;
  background: #fff;
  color: var(--color-text);
  font: inherit;
  font-size: 14px;
  font-weight: 600;
  white-space: nowrap;
  cursor: pointer;

  &:hover,
  &:focus-visible {
    border-color: var(--color-primary);
  }

  &:focus-visible {
    outline: 2px solid var(--color-primary);
    outline-offset: 2px;
  }

  &--selected {
    border-color: var(--color-primary);
    background: var(--color-primary);
    color: #fff;
  }
}
</style>
