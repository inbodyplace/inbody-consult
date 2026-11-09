<script setup>
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faUserCheck } from '@fortawesome/free-solid-svg-icons'
import SimilarMemberCard from '../../molecules/inbody-simulator/SimilarMemberCard.vue'

defineProps({
  members: { type: Array, default: () => [] },
  gender: { type: String, required: true },
})
</script>

<template>
  <section class="similar-members" aria-labelledby="similar-members-title">
    <div class="similar-members__header">
      <div>
        <h2 id="similar-members-title">
          <span class="similar-members__icon" aria-hidden="true">
            <FontAwesomeIcon :icon="faUserCheck" />
          </span>
          Members with similar measurements
        </h2>
        <p>{{ gender === 'female' ? 'Female' : 'Male' }} members with similar measurements.</p>
      </div>
    </div>

    <div class="similar-members__body">
      <SimilarMemberCard v-for="member in members.slice(0, 3)" :key="member.id" :member="member" />
      <div v-if="members.length === 0" class="similar-members__empty" role="status">
        <strong>No similar members</strong>
        <span>No member records can be compared with these measurements.</span>
      </div>
      <p class="similar-members__note">Values shown are actual member measurements.</p>
    </div>
  </section>
</template>

<style scoped lang="scss">
.similar-members {
  border: 1px solid #dfe7f1;
  border-radius: 16px;
  background: #fff;

  &__header {
    display: flex;
    min-height: 74px;
    padding: 20px 20px;
    align-items: flex-start;
    justify-content: space-between;
    gap: 12px;
    border-bottom: 1px solid #eef2f7;

    h2 {
      display: flex;
      margin: 0 0 5px;
      align-items: center;
      gap: 8px;
      color: var(--color-text);
      font-size: 16px;
    }

    p {
      margin: 0;
      color: #75859a;
      font-size: 13px;
      line-height: 1.4;
    }
  }

  &__icon {
    display: grid;
    width: 23px;
    height: 23px;
    place-items: center;
    border-radius: 8px;
    background: #f3f6fa;
    color: var(--color-primary);

    svg {
      width: 16px;
      height: 16px;
    }
  }

  &__badge {
    margin-top: 1px;
    padding: 5px 8px;
    border: 1px solid #e5ebf2;
    border-radius: 14px;
    color: #526171;
    font-size: 12px;
    white-space: nowrap;
  }

  &__body {
    display: flex;
    padding: 18px 20px;
    flex-direction: column;
    gap: 9px;
  }

  &__note {
    margin: 0;
    color: #75859a;
    font-size: 12px;
    line-height: 1.5;
  }

  &__empty {
    display: flex;
    min-height: 120px;
    padding: 20px;
    align-items: center;
    justify-content: center;
    flex-direction: column;
    gap: 7px;
    border: 1px dashed #d8e2ef;
    border-radius: 12px;
    text-align: center;

    strong {
      font-size: 15px;
    }

    span {
      color: #75859a;
      font-size: 13px;
    }
  }

  &__note {
    padding: 2px 3px 0;
  }
}

@media (max-width: 620px) {
  .similar-members {
    &__header {
      padding: 15px;
      flex-wrap: wrap;
    }

    &__body {
      padding: 15px;
    }
  }
}
</style>
