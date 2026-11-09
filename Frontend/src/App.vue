<script setup>
import { RouterLink, RouterView, useRoute } from 'vue-router'

const route = useRoute()
</script>

<template>
  <div class="app-shell">
    <header class="app-header">
      <div class="app-header__inner">
        <span class="brand">InBody Consult</span>
        <nav class="navigation" aria-label="Main navigation">
          <RouterLink :to="{ name: 'simulator' }">Body Composition Forecast</RouterLink>
          <RouterLink
            :to="{ name: 'products' }"
            :class="{ 'navigation__link--active': route.name === 'product-detail' }"
            >Programs</RouterLink
          >
        </nav>
      </div>
    </header>
    <main class="app-content">
      <RouterView />
    </main>
  </div>
</template>

<style scoped lang="scss">
.app-shell {
  display: flex;
  height: 100dvh;
  min-height: 0;
  flex-direction: column;
}

.app-header {
  flex: none;
  position: sticky;
  top: 0;
  z-index: 10;
  border-bottom: 1px solid var(--color-border);
  background: #fff;

  &__inner {
    display: flex;
    width: min(100%, 1168px);
    min-height: 72px;
    margin: 0 auto;
    padding: 0 24px;
    align-items: center;
    justify-content: space-between;
    gap: 24px;
  }
}

.brand {
  color: var(--color-primary);
  font-size: 28px;
  font-weight: 800;
  letter-spacing: -0.04em;
  white-space: nowrap;
}

.navigation {
  display: flex;
  min-width: 0;
  align-items: center;
  gap: clamp(16px, 3vw, 36px);

  a {
    color: var(--color-text);
    font-size: 16px;
    font-weight: 500;
    text-decoration: none;
    line-height: 1.3;
    transition: color 160ms ease;

    &:hover,
    &:focus-visible {
      color: var(--color-primary);
    }

    &:focus-visible {
      outline: 2px solid var(--color-primary);
      outline-offset: 5px;
    }

    &.router-link-exact-active,
    &.navigation__link--active {
      color: var(--color-primary);
      font-weight: 700;
    }
  }
}

.app-content {
  flex: 1;
  min-height: 0;
  overflow: auto;
}

@media (max-width: 800px) {
  .app-header__inner {
    min-height: 0;
    padding-block: 14px;
    flex-wrap: wrap;
    gap: 10px;
  }

  .navigation {
    width: 100%;
    justify-content: space-between;
    gap: 12px;

    a {
      font-size: 14px;
    }
  }
}

@media (max-width: 400px) {
  .brand {
    font-size: 24px;
  }

  .navigation a {
    font-size: 13px;
  }
}
</style>
