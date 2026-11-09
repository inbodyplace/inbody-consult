<script setup>
const props = defineProps({
  member: { type: Object, required: true },
})

const metrics = [
  { key: 'muscleMass', label: 'Skeletal muscle mass', unit: 'kg' },
  { key: 'bodyFatMass', label: 'Body fat mass', unit: 'kg' },
  { key: 'weight', label: 'Weight', unit: 'kg' },
  { key: 'bodyFatPercentage', label: 'Body fat percentage', unit: '%p' },
]

function difference(key) {
  const delta = props.member.after[key] - props.member.before[key]
  return `${delta > 0 ? '+' : ''}${delta.toFixed(1)}`
}
</script>

<template>
  <article class="member-card">
    <div class="member-card__header">
      <span class="member-card__avatar" aria-hidden="true">{{ member.name.split(' ').at(-1) }}</span>
      <div class="member-card__identity">
        <h3>{{ member.name }} <span>· {{ member.gender === 'female' ? 'Female' : 'Male' }}</span></h3>
        <p>{{ member.startDate }} → {{ member.endDate }} · about {{ member.months }} months</p>
      </div>
    </div>

    <div class="member-card__metrics">
      <div v-for="metric in metrics" :key="metric.key" class="member-card__metric">
        <span>{{ metric.label }} ({{ metric.key === 'bodyFatPercentage' ? '%' : 'kg' }})</span>
        <strong>
          {{ member.before[metric.key].toFixed(1) }} → {{ member.after[metric.key].toFixed(1) }}
        </strong>
        <small>{{ difference(metric.key) }} {{ metric.unit }}</small>
      </div>
    </div>
  </article>
</template>

<style scoped lang="scss">
.member-card {
  padding: 16px;
  border: 1px solid #dfe7f1;
  border-radius: 13px;
  background: #fff;

  &__header {
    display: flex;
    align-items: center;
    gap: 9px;
  }

  &__avatar {
    display: grid;
    width: 32px;
    height: 32px;
    flex: none;
    place-items: center;
    border-radius: 50%;
    background: #edf4fb;
    color: var(--color-primary);
    font-size: 14px;
    font-weight: 700;
  }

  &__identity {
    min-width: 0;

    h3 {
      margin: 0 0 2px;
      color: var(--color-text);
      font-size: 14px;
      font-weight: 700;
      overflow-wrap: anywhere;
    }

    h3 span,
    p {
      color: #75859a;
      font-size: 12px;
      font-weight: 400;
    }

    p {
      margin: 0;
      font-size: 12px;
    }
  }

  &__badge {
    margin-left: auto;
    padding: 4px 7px;
    border-radius: 12px;
    background: #f4f7fb;
    color: #526171;
    font-size: 11px;
    white-space: nowrap;
  }

  &__metrics {
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 7px;
    margin-top: 11px;
  }

  &__metric {
    display: flex;
    min-width: 0;
    min-height: 76px;
    padding: 11px;
    flex-direction: column;
    gap: 2px;
    border-radius: 9px;
    background: #f6f9fd;

    span,
    small {
      color: #75859a;
      font-size: 12px;
      line-height: 1.3;
    }

    strong {
      color: #354356;
      font-size: 14px;
      line-height: 1.3;
    }
  }
}

@media (max-width: 620px) {
  .member-card__metrics {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
