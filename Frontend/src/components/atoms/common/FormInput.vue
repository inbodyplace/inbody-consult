<script setup>
defineProps({
  modelValue: { type: [String, Number], default: '' },
  id: { type: String, required: true },
  type: { type: String, default: 'text' },
  placeholder: { type: String, default: '' },
  min: { type: [String, Number], default: undefined },
  step: { type: [String, Number], default: undefined },
  maxlength: { type: [String, Number], default: undefined },
  autocomplete: { type: String, default: undefined },
  inputmode: { type: String, default: undefined },
  autofocus: { type: Boolean, default: false },
})

const emit = defineEmits(['update:modelValue'])

function updateValue(event) {
  emit('update:modelValue', event.target.value)
}
</script>

<template>
  <input
    :id="id"
    class="form-input"
    :type="type"
    :value="modelValue"
    :placeholder="placeholder"
    :min="min"
    :step="step"
    :maxlength="maxlength"
    :autocomplete="autocomplete"
    :inputmode="inputmode"
    :autofocus="autofocus"
    @input="updateValue"
  />
</template>

<style scoped lang="scss">
.form-input {
  display: block;
  width: 100%;
  height: 44px;
  padding: 0 13px;
  border: 1px solid #d8e2ef;
  border-radius: 12px;
  background: #fff;
  color: var(--color-text);
  font: inherit;
  font-size: 14px;

  &::placeholder {
    color: #75859a;
    opacity: 1;
  }

  &:focus {
    border-color: var(--color-primary);
    outline: 2px solid color-mix(in srgb, var(--color-primary) 20%, transparent);
  }
}
</style>
