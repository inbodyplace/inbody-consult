<script setup>
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faChevronDown } from '@fortawesome/free-solid-svg-icons'

defineProps({
  modelValue: { type: String, default: '' },
  id: { type: String, required: true },
  placeholder: { type: String, default: 'Select an option' },
  options: { type: Array, default: () => [] },
  disabled: { type: Boolean, default: false },
})

const emit = defineEmits(['update:modelValue'])
</script>

<template>
  <div class="select-wrap">
    <select
      :id="id"
      :value="modelValue"
      :disabled="disabled"
      @change="emit('update:modelValue', $event.target.value)"
    >
      <option value="" disabled>{{ placeholder }}</option>
      <option v-for="option in options" :key="option.value" :value="option.value">
        {{ option.label }}
      </option>
    </select>
    <FontAwesomeIcon :icon="faChevronDown" aria-hidden="true" />
  </div>
</template>

<style scoped lang="scss">
.select-wrap {
  position: relative;

  select {
    width: 100%;
    height: 46px;
    padding: 0 36px 0 14px;
    appearance: none;
    border: 1px solid #d8e2ef;
    border-radius: 12px;
    background: #fff;
    color: var(--color-text);
    font: inherit;
    font-size: 14px;
    cursor: pointer;

    &:disabled {
      cursor: not-allowed;
    }

    &:has(option:first-child:checked) {
      color: #75859a;
    }

    &:focus {
      border-color: var(--color-primary);
      outline: 2px solid color-mix(in srgb, var(--color-primary) 20%, transparent);
    }
  }

  svg {
    position: absolute;
    top: 15px;
    right: 15px;
    width: 16px;
    height: 16px;
    color: #75859a;
    pointer-events: none;
  }
}
</style>
