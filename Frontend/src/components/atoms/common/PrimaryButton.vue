<script setup>
defineProps({
  disabled: { type: Boolean, default: false },
})
</script>

<template>
  <button class="primary-button" type="button" :disabled="disabled"><slot /></button>
</template>

<style scoped lang="scss">
.primary-button {
  width: 100%;
  min-height: 46px;
  border: 0;
  border-radius: 12px;
  background: var(--color-primary);
  color: #fff;
  font: inherit;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;

  &:disabled {
    opacity: 0.45;
    cursor: not-allowed;
  }

  &:hover:not(:disabled) {
    filter: brightness(0.92);
  }

  &:focus-visible {
    outline: 2px solid var(--color-primary);
    outline-offset: 3px;
  }
}
</style>
