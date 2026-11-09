<script setup>
import FormInput from '../../atoms/common/FormInput.vue'

defineProps({
  modelValue: { type: [String, Number], default: '' },
  id: { type: String, required: true },
  label: { type: String, required: true },
  placeholder: { type: String, default: '' },
})

defineEmits(['update:modelValue'])
</script>

<template>
  <div class="measurement-field">
    <label :for="id">{{ label }}</label>
    <FormInput
      :id="id"
      type="number"
      min="0"
      step="0.1"
      :model-value="modelValue"
      :placeholder="placeholder"
      @update:model-value="$emit('update:modelValue', $event)"
    />
  </div>
</template>

<style scoped lang="scss">
.measurement-field {
  min-width: 0;

  label {
    display: block;
    margin-bottom: 5px;
    color: #75859a;
    font-size: 12px;
  }

  :deep(input) {
    text-align: right;
  }
}
</style>
