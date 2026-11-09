<script setup>
import { ref } from 'vue'
import FormInput from '../../atoms/common/FormInput.vue'
import PrimaryButton from '../../atoms/common/PrimaryButton.vue'

const emit = defineEmits(['cancel', 'add'])
const props = defineProps({
  loading: { type: Boolean, default: false },
  error: { type: String, default: '' },
})
const title = ref('')

function submit() {
  const name = title.value.trim()
  if (name) emit('add', name)
}
</script>

<template>
  <div class="dialog-backdrop">
    <section
      class="product-add-dialog"
      role="dialog"
      aria-modal="true"
      aria-labelledby="product-add-title"
      @keydown.esc="emit('cancel')"
    >
      <h2 id="product-add-title">Add program</h2>
      <label for="product-title">Program name</label>
      <FormInput
        id="product-title"
        v-model="title"
        placeholder="Enter a program name"
        autofocus
        @keydown.enter="submit"
      />
      <p v-if="props.error" class="product-add-dialog__error" role="alert">{{ props.error }}</p>
      <div class="product-add-dialog__actions">
        <PrimaryButton class="product-add-dialog__cancel" @click="emit('cancel')">
          Cancel
        </PrimaryButton>
        <PrimaryButton :disabled="!title.trim() || props.loading" @click="submit">
          {{ props.loading ? 'Adding...' : 'Add program' }}
        </PrimaryButton>
      </div>
    </section>
  </div>
</template>

<style scoped lang="scss">
.dialog-backdrop {
  position: fixed;
  inset: 0;
  z-index: 20;
  display: grid;
  padding: 20px;
  place-items: center;
  background: rgb(31 41 51 / 48%);
}

.product-add-dialog {
  width: min(100%, 440px);
  padding: 28px;
  border: 1px solid #dfe7f1;
  border-radius: 16px;
  background: #fff;
  box-shadow: 0 16px 48px rgb(31 41 51 / 16%);

  h2 {
    margin: 0 0 24px;
    color: var(--color-text);
    font-size: 20px;
  }

  label {
    display: block;
    margin-bottom: 8px;
    color: #526171;
    font-size: 13px;
    font-weight: 700;
  }

  &__actions {
    display: flex;
    gap: 10px;
    margin-top: 24px;
  }

  &__error {
    margin: 10px 0 0;
    color: #bd4545;
    font-size: 13px;
  }

  &__cancel {
    border: 1px solid #dfe7f1;
    background: #fff;
    color: var(--color-text);
  }
}
</style>
