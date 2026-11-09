import { createRouter, createWebHistory } from 'vue-router'
import InbodySimulatorView from '../views/InbodySimulatorView.vue'
import ProductListView from '../views/ProductListView.vue'
import ProductDetailView from '../views/ProductDetailView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', name: 'simulator', component: InbodySimulatorView },
    { path: '/products', name: 'products', component: ProductListView },
    { path: '/products/:id', name: 'product-detail', component: ProductDetailView },
  ],
})

export default router
