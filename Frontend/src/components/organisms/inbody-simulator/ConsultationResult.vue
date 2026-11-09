<script setup>
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faCircleInfo, faFileLines } from '@fortawesome/free-solid-svg-icons'
import ResultMetric from '../../molecules/inbody-simulator/ResultMetric.vue'

defineProps({
  result: { type: Object, default: null },
})

const metrics = [
  { key: 'muscleMass', label: 'Skeletal muscle mass', unit: 'kg' },
  { key: 'bodyFatMass', label: 'Body fat mass', unit: 'kg' },
  { key: 'weight', label: 'Weight', unit: 'kg' },
  { key: 'bodyFatPercentage', label: 'Body fat percentage', unit: '%' },
]
</script>

<template>
  <section v-if="!result" class="result-empty" aria-label="Consultation results guide">
    <div class="result-empty__content">
      <div class="result-empty__icon" aria-hidden="true">
        <FontAwesomeIcon :icon="faFileLines" />
      </div>
      <h2>Select View forecast to see projected InBody changes</h2>
      <p>
        Enter consultation details on the left, then select View forecast.<br />Changes in all four metrics will
        appear together.
      </p>
      <ol class="result-empty__steps">
        <li><span>1</span>Select a program</li>
        <li><span>2</span>Enter InBody data</li>
        <li><span>3</span>Review results</li>
      </ol>
      <p class="result-empty__note">
        Explore projected changes based on the selected program and current measurements.
      </p>
    </div>
  </section>

  <section v-else class="result-card" aria-labelledby="result-title">
    <div class="result-card__bar">
      <span class="result-card__eyebrow"><span class="result-card__dot" />Consultation summary</span>
      <span class="result-card__badge">Projection · not a measurement</span>
    </div>
    <div class="result-card__body">
      <div class="result-card__intro">
        <div>
          <p>
            <strong>{{ result.customerName }}</strong> ·
            {{ result.gender === 'female' ? 'Female' : 'Male' }} · Four InBody metrics
          </p>
          <h2 id="result-title">{{ result.productName || 'Consultation preview' }}</h2>
        </div>
        <span class="result-card__period">{{ result.period }}-month forecast</span>
      </div>

      <div class="result-card__metrics">
        <ResultMetric
          v-for="metric in metrics"
          :key="metric.key"
          :label="metric.label"
          :unit="metric.unit"
          :current="result.current[metric.key]"
          :projected="result.projected?.[metric.key] ?? null"
          :period="result.period"
        />
      </div>

      <p v-if="!result.projected" class="result-card__unavailable" role="status">
        {{ result.completedCount }} members of this gender have complete measurement pairs. A forecast
        requires at least three.
      </p>

      <div class="result-card__footer">
        <p>Forecast for {{ result.period }} months from the current measurements.</p>
        <p class="result-card__notice">
          <FontAwesomeIcon :icon="faCircleInfo" aria-hidden="true" />
          Forecasts do not guarantee actual changes or program outcomes.
        </p>
      </div>
    </div>
  </section>
</template>

<style scoped lang="scss">
.result-empty,
.result-card {
  min-width: 0;
  background: #fff;
  border-radius: 16px;
}

.result-empty {
  display: grid;
  min-height: 480px;
  padding: 32px;
  place-items: center;
  border: 1px dashed #d8e2ef;
  text-align: center;

  &__content {
    width: min(100%, 450px);
  }

  &__icon {
    display: grid;
    width: 52px;
    height: 52px;
    margin: 0 auto 20px;
    place-items: center;
    border-radius: 16px;
    background: #edf4fb;
    color: var(--color-primary);
    font-size: 28px;
  }

  h2 {
    max-width: 500px;
    margin: 0 auto 9px;
    font-size: 19px;
    line-height: 1.4;
  }

  p {
    margin: 0;
    color: #75859a;
    font-size: 13px;
    line-height: 1.6;
  }

  &__steps {
    display: flex;
    gap: 8px;
    margin: 24px 0 21px;
    padding: 0;
    list-style: none;
  }

  &__steps li {
    display: flex;
    flex: 1;
    min-width: 0;
    align-items: center;
    gap: 7px;
    padding: 8px 10px;
    border: 1px solid #dfe7f1;
    border-radius: 12px;
    background: #f8faff;
    color: #526171;
    font-size: 12px;
    line-height: 1.3;
  }

  &__steps span {
    display: grid;
    width: 19px;
    height: 19px;
    flex: none;
    place-items: center;
    border-radius: 50%;
    background: #e7edf5;
    font-size: 11px;
    font-weight: 700;
  }

  p.result-empty__note {
    font-size: 11px;
  }
}

.result-card {
  border: 1px solid #dfe7f1;
  box-shadow: 0 8px 24px rgb(31 41 51 / 4%);

  &__bar {
    display: flex;
    min-height: 52px;
    padding: 0 30px;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    flex-wrap: wrap;
    border-bottom: 1px solid #eef2f7;
  }

  &__eyebrow {
    display: flex;
    align-items: center;
    gap: 8px;
    color: var(--color-primary);
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 0.06em;
    line-height: 1.3;
  }

  &__dot {
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: var(--color-primary);
  }

  &__badge {
    padding: 5px 9px;
    border-radius: 20px;
    background: #f3f6fa;
    color: #526171;
    font-size: 11px;
    line-height: 1.3;
  }

  &__body {
    padding: 30px;
  }

  &__intro {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
  }

  &__intro > div {
    min-width: 0;
  }

  &__intro p {
    margin: 0 0 5px;
    color: #526171;
    font-size: 13px;
  }

  &__intro h2 {
    margin: 0;
    font-size: 18px;
    overflow-wrap: anywhere;
  }

  &__period {
    padding: 8px 11px;
    border-radius: 20px;
    background: #edf4fb;
    color: var(--color-primary);
    font-size: 12px;
    font-weight: 700;
    white-space: nowrap;
  }

  &__metrics {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 10px;
    margin-top: 20px;
  }

  &__footer {
    margin-top: 20px;
    padding-top: 17px;
    border-top: 1px solid #dfe7f1;
  }

  &__unavailable {
    margin: 14px 0 0;
    padding: 12px 14px;
    border-radius: 10px;
    background: #f6f9fd;
    color: #526171;
    font-size: 12px;
    line-height: 1.5;
  }

  &__footer > p {
    margin: 0;
    color: #526171;
    font-size: 12px;
    line-height: 1.5;
  }

  &__footer > p.result-card__notice {
    margin-top: 11px;
    padding: 12px 14px;
    border-radius: 12px;
    background: #f4f7fb;
  }
}

@media (max-width: 620px) {
  .result-empty {
    min-height: 400px;
    padding: 22px;

    &__steps {
      flex-wrap: wrap;

      li {
        flex-basis: 130px;
      }
    }
  }

  .result-card {
    &__bar {
      padding-inline: 18px;
    }

    &__body {
      padding: 20px 18px;
    }

    &__intro {
      align-items: flex-start;
      flex-direction: column;
    }

    &__metrics {
      grid-template-columns: 1fr;
    }
  }
}

@media (max-width: 420px) {
  .result-empty__steps li {
    flex-basis: 100%;
  }
}
</style>
