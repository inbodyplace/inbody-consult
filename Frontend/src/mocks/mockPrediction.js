// Fixed sample response for the forecast card before API integration.
export const mockPredictionResponse = {
  customerName: 'Sample member',
  gender: 'female',
  productName: 'Weight management program',
  period: 1,
  current: {
    muscleMass: 22.8,
    bodyFatMass: 21.4,
    weight: 74.6,
    bodyFatPercentage: 28.7,
  },
  projected: {
    muscleMass: 23.0,
    bodyFatMass: 20.5,
    weight: 73.8,
    bodyFatPercentage: 27.8,
  },
}
