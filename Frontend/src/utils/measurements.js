const metricKeys = ['smm', 'bfm', 'weight', 'pbf']

export function measurementDate(value) {
  if (!value) return null
  const normalized = value.replaceAll('.', '-')
  const date = new Date(`${normalized}T00:00:00Z`)
  return Number.isNaN(date.getTime()) ? null : date
}

export function isCompletedMember(member) {
  if (typeof member.includedInAverage === 'boolean') return member.includedInAverage
  const start = measurementDate(member.startDate)
  const end = measurementDate(member.endDate)
  if (!start || !end || (end - start) / 86_400_000 < 30) return false
  return metricKeys.every(
    (key) => Number.isFinite(member.before?.[key]) && Number.isFinite(member.after?.[key]),
  )
}

export function mapGender(gender) {
  return gender === 'F' || gender === 'female' ? 'female' : 'male'
}

export function mapMeasurement(measurement) {
  if (!measurement) return null
  return {
    smm: measurement.smm,
    bfm: measurement.bfm,
    weight: measurement.weight,
    pbf: measurement.pbf,
  }
}
