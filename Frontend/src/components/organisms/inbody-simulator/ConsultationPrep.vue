<script setup>
import { computed, reactive, watch } from 'vue'
import ChoiceButton from '../../atoms/inbody-simulator/ChoiceButton.vue'
import FormInput from '../../atoms/common/FormInput.vue'
import FormSelect from '../../atoms/inbody-simulator/FormSelect.vue'
import PrimaryButton from '../../atoms/common/PrimaryButton.vue'
import MeasurementField from '../../molecules/inbody-simulator/MeasurementField.vue'

const props = defineProps({
  products: { type: Array, default: () => [] },
  productsLoading: { type: Boolean, default: false },
  productsError: { type: String, default: '' },
  searching: { type: Boolean, default: false },
  initialProductId: { type: [String, Number], default: '' },
})
const emit = defineEmits(['preview'])

const periods = [1, 2, 3, 6]

const form = reactive({
  product: '',
  customerName: '',
  gender: '',
  muscleMass: '',
  bodyFatMass: '',
  weight: '',
  bodyFatPercentage: '',
  period: 1,
})

watch(
  () => props.initialProductId,
  (productId) => {
    if (productId) form.product = String(productId)
  },
  { immediate: true },
)

const selectedProduct = computed(() =>
  props.products.find((product) => product.value === form.product),
)
const previewDisabled = computed(() => {
  const values = [form.muscleMass, form.bodyFatMass, form.weight, form.bodyFatPercentage]
  const measurementsIncomplete = values.some(
    (value) => value === '' || !Number.isFinite(Number(value)) || Number(value) < 0,
  )

  return (
    props.productsLoading ||
    props.searching ||
    Boolean(props.productsError) ||
    !selectedProduct.value ||
    !form.customerName.trim() ||
    !form.gender ||
    measurementsIncomplete
  )
})

function showPreview() {
  if (previewDisabled.value) {
    return
  }

  emit('preview', {
    productId: form.product,
    productName: selectedProduct.value.label,
    customerName: form.customerName.trim(),
    gender: form.gender,
    period: form.period,
    current: {
      muscleMass: Number(form.muscleMass),
      bodyFatMass: Number(form.bodyFatMass),
      weight: Number(form.weight),
      bodyFatPercentage: Number(form.bodyFatPercentage),
    },
  })
}
</script>

<template>
  <div class="consultation-prep">
      <section class="consultation-prep__card" aria-labelledby="prep-title">
      <h2 id="prep-title" class="consultation-prep__title">Prepare consultation</h2>

      <div class="form-section">
        <h3>1. Select a program</h3>
        <p>Choose a program for the consultation.</p>
        <FormSelect
          id="product"
          v-model="form.product"
          :options="props.products"
          :disabled="props.productsLoading || Boolean(props.productsError) || props.products.length === 0"
          :placeholder="props.productsLoading ? 'Loading programs...' : 'Select a program'"
        />
        <p v-if="props.productsError" class="product-message" role="alert">{{ props.productsError }}</p>
        <primary-button v-else-if="!props.productsLoading && props.products.length === 0" class="product-message">
          No programs are available yet.
        </primary-button>
      </div>

      <div class="form-section">
        <h3>2. Client details</h3>
        <p>Enter the name and gender to show on the card.</p>
        <FormInput id="customer-name" v-model="form.customerName" placeholder="e.g. Alex" />
        <div class="gender-options" role="group" aria-label="Gender">
        <ChoiceButton :selected="form.gender === 'female'" @click="form.gender = 'female'">Female</ChoiceButton>
        <ChoiceButton :selected="form.gender === 'male'" @click="form.gender = 'male'">Male</ChoiceButton>
        </div>
      </div>

      <div class="form-section">
        <h3>3. Enter InBody measurements</h3>
        <p>Enter all four current measurements for the client.</p>
        <div class="measurements">
        <MeasurementField id="muscle-mass" v-model="form.muscleMass" label="Skeletal muscle mass (kg)" placeholder="25.0" />
        <MeasurementField id="body-fat-mass" v-model="form.bodyFatMass" label="Body fat mass (kg)" placeholder="25.0" />
        <MeasurementField id="weight" v-model="form.weight" label="Weight (kg)" placeholder="58.2" />
        <MeasurementField id="body-fat-percentage" v-model="form.bodyFatPercentage" label="Body fat percentage (%)" placeholder="31.6" />
        </div>
      </div>

      <div class="form-section form-section--last">
        <h3>4. Forecast period</h3>
        <p>Choose how many months ahead to forecast.</p>
        <div class="period-options" role="group" aria-label="Forecast period">
          <ChoiceButton
            v-for="period in periods"
            :key="period"
            :selected="form.period === period"
            @click="form.period = period"
          >
            {{ period }} months
          </ChoiceButton>
        </div>
      </div>
    </section>
    <PrimaryButton class="preview-button" :disabled="previewDisabled" @click="showPreview">
      {{ props.searching ? 'Loading results...' : 'View forecast' }}
    </PrimaryButton>
  </div>
</template>

<style scoped lang="scss">
.consultation-prep {
  width: min(100%, 400px);

  &__card {
    padding: 20px;
    border: 1px solid #dfe7f1;
    border-radius: 16px;
    background: #fff;
  }

  &__title {
    margin: 0 0 17px;
    color: #75859a;
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 0.08em;
  }
}

.form-section {
  margin-bottom: 20px;

  h3 {
    margin: 0 0 4px;
    color: var(--color-text);
    font-size: 14px;
    font-weight: 700;
  }

  p {
    margin: 0 0 8px;
    color: #75859a;
    font-size: 13px;
    line-height: 1.45;
  }

  .helper-text {
    margin: 8px 0 0;
    font-size: 12px;
  }

  .product-message {
    margin: 7px 0 0;
    font-size: 12px;
  }

  &--last {
    margin-bottom: 0;
  }
}

.gender-options,
.measurements {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 7px;
}

.gender-options {
  margin-top: 7px;

  :deep(button) {
    border-radius: 12px;
  }
}

.measurements {
  row-gap: 8px;
}

.period-options {
  display: flex;
  flex-wrap: wrap;
  gap: 7px;
}

.preview-button {
  margin-top: 12px;
}
</style>
