import { reactive } from 'vue'
import { api } from '../api/client.js'
import { mapGender, mapMeasurement } from '../utils/measurements.js'

export const products = reactive([])

function formatDate(value) {
  return value?.replaceAll('-', '.') ?? ''
}

function formatAverage(value) {
  if (!Number.isFinite(value)) return '—'
  return `${value > 0 ? '+' : ''}${value.toFixed(2)}`
}

function fromApiProduct(item) {
  const average = item.monthlyAverageChange ?? item.stats?.overall?.monthlyAverageChange
  const completedCount = item.completedCount ?? item.stats?.overall?.completedCount ?? 0
  return {
    id: item.id,
    name: item.name,
    registeredAt: formatDate(item.registeredAt),
    muscle: formatAverage(average?.smm),
    fat: formatAverage(average?.bfm),
    weight: formatAverage(average?.weight),
    percentage: formatAverage(average?.pbf),
    before: completedCount,
    included: item.memberCount ?? 0,
    excluded: Math.max(0, (item.memberCount ?? 0) - (item.completedCount ?? 0)),
    monthlyAverageChange: average,
    stats: item.stats ?? null,
    members: [],
    membersLoaded: false,
  }
}

function fromApiMember(item) {
  return {
    id: item.id,
    name: item.name,
    gender: mapGender(item.gender),
    startDate: formatDate(item.startDate),
    endDate: formatDate(item.endDate),
    before: mapMeasurement(item.before),
    after: mapMeasurement(item.after),
    includedInAverage: item.includedInAverage,
  }
}

function applyMembers(product, members, totalItems) {
  product.members = members
  product.membersLoaded = true
  product.included = totalItems
}

function mergeProduct(product) {
  const existing = products.find((item) => item.id === product.id)
  if (!existing) {
    products.push(product)
    return product
  }

  const members = existing.members
  const membersLoaded = existing.membersLoaded
  Object.assign(existing, product, { members, membersLoaded })
  return existing
}

export async function loadProducts() {
  const items = await api.get('/api/products')
  const previous = new Map(products.map((product) => [product.id, product]))
  products.splice(
    0,
    products.length,
    ...items.map((item) => {
      const product = fromApiProduct(item)
      const cached = previous.get(item.id)
      if (cached?.membersLoaded) {
        product.members = cached.members
        product.membersLoaded = true
      }
      return product
    }),
  )
  return products
}

export async function loadProductMembers(productId) {
  const product = products.find((item) => item.id === Number(productId))
  const members = []
  let page = 1
  let totalPages = 1
  let totalItems = 0

  do {
    const result = await api.get(`/api/products/${productId}/members?page=${page}&size=100`)
    members.push(...result.items.map(fromApiMember))
    totalItems = result.totalItems
    totalPages = result.totalPages
    page += 1
  } while (page <= totalPages)

  if (product) applyMembers(product, members, totalItems)
  return members
}

export async function loadProductDetail(productId) {
  const detail = await api.get(`/api/products/${productId}`)
  return mergeProduct(fromApiProduct(detail))
}

export async function createProduct(name) {
  const created = await api.post('/api/products', { name })
  const product = fromApiProduct({ ...created, memberCount: 0, completedCount: 0 })
  products.push(product)
  return product
}

export async function addProductMember(productId, userToken) {
  const member = await api.post(`/api/products/${productId}/members`, { userToken })
  await Promise.all([loadProductMembers(productId), loadProductDetail(productId)])
  return member
}

export async function removeProductMember(productId, memberId) {
  await api.delete(`/api/products/${productId}/members/${encodeURIComponent(memberId)}`)
  await Promise.all([loadProductMembers(productId), loadProductDetail(productId)])
}
