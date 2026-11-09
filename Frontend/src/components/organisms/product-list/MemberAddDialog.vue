<script setup>
import { computed, ref } from 'vue'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faCircleInfo } from '@fortawesome/free-solid-svg-icons'
import FormInput from '../../atoms/common/FormInput.vue'
import PrimaryButton from '../../atoms/common/PrimaryButton.vue'

const emit = defineEmits(['cancel', 'add'])
const props = defineProps({
  loading: { type: Boolean, default: false },
  error: { type: String, default: '' },
})
const phone = ref('')
const digits = computed(() => phone.value.replace(/\D/g, ''))
const isValid = computed(() =>
  /^(?:\+|\(|[0-9])[0-9\s().-]*[0-9]$/.test(phone.value.trim()) &&
  phone.value.trim().length <= 32 &&
  digits.value.length >= 7 &&
  digits.value.length <= 15,
)

function updatePhone(value) {
  phone.value = value
}

function submit() {
  if (isValid.value) emit('add', digits.value)
}
</script>

<template>
  <div class="dialog-backdrop" @click.self="emit('cancel')">
    <section
      class="member-add-dialog"
      role="dialog"
      aria-modal="true"
      aria-labelledby="member-add-title"
      @keydown.esc="emit('cancel')"
    >
      <h2 id="member-add-title">Add member</h2>
      <p class="member-add-dialog__intro">Enter the mobile number registered with InBody.</p>
      <label for="member-phone">Mobile number</label>
      <FormInput
        id="member-phone"
        :model-value="phone"
        type="tel"
        inputmode="tel"
        autocomplete="tel"
        maxlength="32"
        placeholder="Enter mobile number"
        autofocus
        @update:model-value="updatePhone"
        @keydown.enter="submit"
      />
      <p class="member-add-dialog__hint">
        <FontAwesomeIcon :icon="faCircleInfo" aria-hidden="true" />
        Include the country code if it is part of your InBody registration.
      </p>
      <p v-if="phone && !isValid" class="member-add-dialog__validation" role="status">
        Enter a phone number with 7 to 15 digits.
      </p>
      <p v-if="props.error" class="member-add-dialog__validation" role="alert">{{ props.error }}</p>
      <div class="member-add-dialog__actions">
        <PrimaryButton class="member-add-dialog__cancel" @click="emit('cancel')"
          >Cancel</PrimaryButton
        >
        <PrimaryButton :disabled="!isValid || props.loading" @click="submit">
          {{ props.loading ? 'Adding...' : 'Add member' }}
        </PrimaryButton>
      </div>
    </section>
  </div>
</template>

<style scoped lang="scss">
.dialog-backdrop {
  position: fixed;
  inset: 0;
  z-index: 30;
  display: grid;
  padding: 20px;
  place-items: center;
  background: rgb(31 41 51 / 48%);
}

.member-add-dialog {
  width: min(100%, 440px);
  padding: 28px;
  border: 1px solid #dfe7f1;
  border-radius: 16px;
  background: #fff;
  box-shadow: 0 16px 48px rgb(31 41 51 / 16%);

  h2 {
    margin: 0 0 8px;
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

  &__intro {
    margin: 0 0 24px;
    color: #75859a;
    font-size: 14px;
    line-height: 1.5;
  }

  &__hint,
  &__validation {
    display: flex;
    align-items: center;
    gap: 6px;
    margin: 9px 0 0;
    font-size: 12px;
    line-height: 1.5;
  }

  &__hint {
    color: #75859a;
  }

  &__validation {
    color: #bd4545;
  }

  &__actions {
    display: flex;
    gap: 10px;
    margin-top: 24px;
  }

  &__cancel {
    border: 1px solid #dfe7f1;
    background: #fff;
    color: var(--color-text);
  }
}
</style>
