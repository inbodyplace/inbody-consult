<script setup>
import { computed, ref } from 'vue'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faUser, faUserPlus } from '@fortawesome/free-solid-svg-icons'
import ProductMemberRow from '../../molecules/product-list/ProductMemberRow.vue'
import MemberAddDialog from './MemberAddDialog.vue'
import { addProductMember, removeProductMember } from '../../../stores/products.js'
import { isCompletedMember } from '../../../utils/measurements.js'
const props = defineProps({ product: { type: Object, required: true } })
const isAddOpen = ref(false)
const isAdding = ref(false)
const apiError = ref('')
const completedCount = computed(() =>
  props.product.stats?.overall?.completedCount ?? props.product.before,
)
const sortedMembers = computed(() =>
  props.product.members
    .map((member, index) => ({ member, index, completed: isCompletedMember(member) }))
    .sort((a, b) => Number(b.completed) - Number(a.completed) || a.index - b.index)
    .map(({ member }) => member),
)

function openAddDialog() {
  isAddOpen.value = true
}

function closeAddDialog() {
  isAddOpen.value = false
  apiError.value = ''
}

async function addMember(userToken) {
  apiError.value = ''
  isAdding.value = true
  try {
    await addProductMember(props.product.id, userToken)
    closeAddDialog()
  } catch (error) {
    apiError.value = error.message
  } finally {
    isAdding.value = false
  }
}

async function removeMember(member) {
  if (!window.confirm(`Remove ${member.name} from this program?`)) return
  apiError.value = ''
  try {
    await removeProductMember(props.product.id, member.id)
  } catch (error) {
    apiError.value = error.message
  }
}
</script>

<template>
  <section class="members-panel" aria-labelledby="members-title">
    <div class="members-panel__header">
      <div>
        <h2 id="members-title">Member measurements</h2>
        <p>Review changes by metric. Parentheses show initial → follow-up values.</p>
      </div>
      <button class="members-panel__add" type="button" @click="openAddDialog">
        <FontAwesomeIcon :icon="faUserPlus" aria-hidden="true" />
        Add member
      </button>
    </div>
    <p v-if="apiError" class="members-panel__error" role="alert">{{ apiError }}</p>
    <div v-if="product.members.length === 0" class="members-panel__empty">
      <span class="members-panel__empty-icon" aria-hidden="true">
        <FontAwesomeIcon :icon="faUser" />
      </span>
      <strong>No members registered yet</strong>
      <p>Add members to calculate average changes.</p>
    </div>
    <div v-else class="members-panel__list">
      <table>
        <colgroup>
          <col class="members-panel__col-member" />
          <col class="members-panel__col-date" />
          <col class="members-panel__col-date" />
          <col v-for="key in 4" :key="key" class="members-panel__col-metric" />
          <col class="members-panel__col-status" />
          <col class="members-panel__col-delete" />
        </colgroup>
        <thead>
          <tr>
            <th scope="col">Member</th>
            <th scope="col">Initial measurement</th>
            <th scope="col">Latest measurement</th>
            <th scope="col">Skeletal muscle mass</th>
            <th scope="col">Body fat mass</th>
            <th scope="col">Weight</th>
            <th scope="col">Body fat percentage</th>
            <th scope="col">Included in average</th>
            <th scope="col"><span class="sr-only">Actions</span></th>
          </tr>
        </thead>
        <tbody>
          <ProductMemberRow
            v-for="member in sortedMembers"
            :key="member.id"
            :member="member"
            @remove="removeMember"
          />
        </tbody>
      </table>
    </div>
    <p class="members-panel__notice">
      Only members with complete initial and follow-up values for all four metrics contribute to the
      average. Excluded members are not among the {{ completedCount }} complete members shown on the
      consultation card.
    </p>
    <MemberAddDialog
      v-if="isAddOpen"
      :loading="isAdding"
      :error="apiError"
      @cancel="closeAddDialog"
      @add="addMember"
    />
  </section>
</template>

<style scoped lang="scss">
.members-panel {
  border: 1px solid #dfe7f1;
  border-radius: 15px;
  background: #fff;
  overflow: hidden;

  &__header {
    display: flex;
    min-height: 66px;
    padding: 16px 18px;
    align-items: center;
    justify-content: space-between;
    gap: 14px;
    border-bottom: 1px solid #eef2f7;

    h2 {
      margin: 0 0 4px;
      font-size: 17px;
    }

    p {
      margin: 0;
      color: #75859a;
      font-size: 13px;
    }
  }

  &__add {
    display: inline-flex;
    flex: none;
    align-items: center;
    gap: 7px;
    padding: 9px 13px;
    border: 1px solid #d8e2ef;
    border-radius: 12px;
    background: #fff;
    color: var(--color-text);
    font: inherit;
    font-size: 14px;
    font-weight: 700;
    cursor: pointer;

    svg {
      width: 20px;
      height: 20px;
      flex: none;
    }
  }

  &__empty {
    display: flex;
    min-height: 184px;
    align-items: center;
    justify-content: center;
    flex-direction: column;
    gap: 4px;
    text-align: center;

    strong {
      margin-top: 7px;
      font-size: 15px;
    }

    p {
      margin: 0;
      color: #75859a;
      font-size: 13px;
    }
  }

  &__empty-icon {
    display: grid;
    width: 60px;
    height: 60px;
    place-items: center;
    border-radius: 16px;
    background: #f6f9fd;
    color: #75859a;

    svg {
      width: 34px;
      height: 34px;
    }
  }

  &__list {
    max-height: 420px;
    overflow: auto;

    table {
      width: 100%;
      min-width: 1180px;
      border-collapse: collapse;
      table-layout: fixed;
    }

    th {
      padding: 11px 10px;
      border-bottom: 1px solid #eef2f7;
      color: #75859a;
      font-size: 11px;
      font-weight: 500;
      text-align: center;
      line-height: 1.3;
      overflow-wrap: anywhere;
    }

    th:first-child {
      text-align: left;
    }
  }

  &__col-member {
    width: 14%;
  }

  &__col-date {
    width: 11%;
  }

  &__col-metric {
    width: 11%;
  }

  &__col-status {
    width: 14%;
  }

  &__col-delete {
    width: 6%;
  }

  &__notice {
    margin: 0;
    padding: 13px 18px;
    border-top: 1px solid #eef2f7;
    color: #75859a;
    font-size: 13px;
    line-height: 1.5;
  }

  &__error {
    margin: 12px 18px 0;
    color: #bd4545;
    font-size: 13px;
  }
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
}

@media (max-width: 700px) {
  .members-panel__header {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
