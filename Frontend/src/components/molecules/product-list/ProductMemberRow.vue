<script setup>
import { computed } from 'vue'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faCheck, faMinus, faTrashCan } from '@fortawesome/free-solid-svg-icons'
import { isCompletedMember } from '../../../utils/measurements.js'

const props = defineProps({
  member: { type: Object, required: true },
})

const metrics = [
  { api: 'smm', mock: 'muscleMass' },
  { api: 'bfm', mock: 'bodyFatMass' },
  { api: 'weight', mock: 'weight' },
  { api: 'pbf', mock: 'bodyFatPercentage' },
]
function valueFor(values, metric) {
  return values?.[metric.api] ?? values?.[metric.mock]
}
const included = computed(
  () => isCompletedMember(props.member),
)

const emit = defineEmits(['remove'])

function delta(metric) {
  if (!included.value) return '–'
  const change = valueFor(props.member.after, metric) - valueFor(props.member.before, metric)
  return `${change > 0 ? '+' : ''}${change.toFixed(1)}`
}
</script>

<template>
  <tr class="member-row">
    <td class="member-row__identity">
      <strong>{{ member.name }}</strong>
      <span
        >{{ member.gender === 'female' || member.gender === 'F' ? 'Female' : 'Male' }} · Program
        member</span
      >
    </td>
    <td>{{ member.startDate || '–' }}</td>
    <td>{{ member.endDate || '–' }}</td>
    <td v-for="metric in metrics" :key="metric.api" class="member-row__metric">
      <strong>{{ delta(metric) }}</strong>
      <span v-if="included"
        >{{ valueFor(member.before, metric).toFixed(1) }} →
        {{ valueFor(member.after, metric).toFixed(1) }}</span
      >
      <span v-else>No measurement</span>
    </td>
    <td>
      <span :class="['member-row__status', { 'member-row__status--excluded': !included }]">
        <FontAwesomeIcon :icon="included ? faCheck : faMinus" aria-hidden="true" />
        {{ included ? 'Included' : 'Excluded' }}
      </span>
    </td>
    <td class="member-row__delete">
      <button type="button" :aria-label="`Remove ${member.name}`" @click="emit('remove', member)">
        <FontAwesomeIcon :icon="faTrashCan" aria-hidden="true" />
      </button>
    </td>
  </tr>
</template>

<style scoped lang="scss">
.member-row {
  border-bottom: 1px solid #eef2f7;

  &:last-child {
    border-bottom: 0;
  }

  td {
    padding: 12px 20px;
    color: #526171;
    font-size: 12px;
    white-space: nowrap;
  }

  td:nth-child(n + 2):nth-child(-n + 8) {
    text-align: center;
  }

  &__identity,
  &__metric {
    strong,
    span {
      display: block;
    }

    strong {
      color: var(--color-text);
      font-size: 13px;
    }

    span {
      margin-top: 4px;
      color: #75859a;
      font-size: 11px;
    }
  }

  &__metric {
    text-align: center;

    strong {
      font-size: 12px;
    }
  }

  &__status {
    display: inline-flex;
    align-items: center;
    gap: 5px;
    padding: 4px 8px;
    border-radius: 16px;
    background: #edf4fb;
    color: var(--color-primary);
    font-size: 11px;
    font-weight: 700;

    &--excluded {
      background: #edf1f5;
      color: #526171;
    }
  }

  &__delete {
    text-align: right;

    button {
      padding: 4px;
      border: 0;
      background: none;
      color: #9aa9bc;
      cursor: pointer;
    }
  }
}
</style>
