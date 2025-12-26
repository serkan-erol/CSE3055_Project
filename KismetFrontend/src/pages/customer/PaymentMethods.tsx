import { useState, useEffect } from 'react'
import { paymentMethodApi, sessionApi } from '../../services/api'

interface PaymentMethod {
  spmID?: number
  SPMID?: number // Handle both camelCase and PascalCase
  customerID?: number
  CustomerID?: number
  cardNumber?: string
  CardNumber?: string
  cardType?: string
  CardType?: string
  cardExpirationDate?: string
  CardExpirationDate?: string
  recordExpirationDate?: string
  RecordExpirationDate?: string
  createdAt?: string
  CreatedAt?: string
  lastUpdatedAt?: string
  LastUpdatedAt?: string
}

const PaymentMethods = () => {
  const [paymentMethods, setPaymentMethods] = useState<PaymentMethod[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [showForm, setShowForm] = useState(false)
  const [customerId, setCustomerId] = useState<number | null>(null)
  const [deletingId, setDeletingId] = useState<number | null>(null)

  const [formData, setFormData] = useState({
    cardNumber: '',
    cardType: 'Debit',
    cardExpirationDate: '',
  })

  useEffect(() => {
    // Try to load from sessionStorage first
    const stored = sessionStorage.getItem('paymentMethods')
    if (stored) {
      try {
        const parsed = JSON.parse(stored)
        if (Array.isArray(parsed) && parsed.length > 0) {
          setPaymentMethods(parsed)
          setLoading(false)
        }
      } catch (e) {
        console.error('Error parsing stored payment methods:', e)
      }
    }
    // Always fetch fresh data from API
    loadData()
  }, [])

  const loadData = async () => {
    try {
      setLoading(true)
      setError('')
      const currentUser = await sessionApi.getCurrentUser()
      if (!currentUser.userId) {
        setError('Unable to identify customer. Please log in again.')
        return
      }
      setCustomerId(currentUser.userId)
      const methods = await paymentMethodApi.getAllByCustomerId(currentUser.userId)
      // Use data as-is - ASP.NET Core should return camelCase
      const data = Array.isArray(methods) ? methods : []
      // Store in sessionStorage for persistence
      sessionStorage.setItem('paymentMethods', JSON.stringify(data))
      setPaymentMethods(data)
    } catch (err: any) {
      setError(err.response?.data?.error || err.message || 'Failed to load payment methods')
    } finally {
      setLoading(false)
    }
  }

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target
    setFormData((prev) => ({ ...prev, [name]: value }))
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!customerId) {
      setError('Unable to identify customer')
      return
    }

    try {
      setError('')
      setSuccessMessage('')

      // Create new
      await paymentMethodApi.create({
        customerID: customerId,
        cardNumber: formData.cardNumber,
        cardType: formData.cardType,
        cardExpirationDate: formData.cardExpirationDate,
      })
      setSuccessMessage('Payment method added successfully!')

      setShowForm(false)
      setFormData({ cardNumber: '', cardType: 'Debit', cardExpirationDate: '' })
      await loadData() // This will update sessionStorage
    } catch (err: any) {
      setError(err.response?.data?.error || err.message || 'Failed to save payment method')
    }
  }

  const handleCancel = () => {
    setShowForm(false)
    setFormData({ cardNumber: '', cardType: 'Debit', cardExpirationDate: '' })
    setError('')
  }

  const handleDelete = async (spmId: number | null | undefined) => {
    if (!spmId || spmId === null || spmId === undefined || isNaN(Number(spmId))) {
      setError('Invalid payment method ID')
      return
    }

    if (!window.confirm('Are you sure you want to delete this payment method?')) {
      return
    }

    const idToDelete = Number(spmId)
    try {
      setDeletingId(idToDelete)
      setError('')
      setSuccessMessage('')
      console.log('Deleting payment method with ID:', idToDelete)
      await paymentMethodApi.delete(idToDelete)
      setSuccessMessage('Payment method deleted successfully!')
      // Clear deleting state before reloading
      setDeletingId(null)
      await loadData() // Reload data to update the list
    } catch (err: any) {
      console.error('Error deleting payment method:', err)
      setError(err.response?.data?.error || err.message || 'Failed to delete payment method')
      setDeletingId(null) // Clear on error
    }
  }

  const formatCardNumber = (cardNumber: string) => {
    // Mask all but last 4 digits
    if (cardNumber.length <= 4) return cardNumber
    return '**** **** **** ' + cardNumber.slice(-4)
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
  }

  // Helper function to extract ID from any object, checking all possible variations
  const extractPaymentMethodId = (method: any): number | null => {
    // Try all common variations
    const possibleKeys = ['spmID', 'SPMID', 'spmId', 'SPMId', 'sPMID', 'SpmID', 'id', 'ID', 'Id']
    
    for (const key of possibleKeys) {
      if (method[key] !== null && method[key] !== undefined && method[key] !== '') {
        const value = Number(method[key])
        if (!isNaN(value) && value > 0) {
          return value
        }
      }
    }
    
    // If not found, search for any key containing 'id' (case insensitive)
    const idKey = Object.keys(method).find(key => 
      key.toLowerCase().includes('id') && 
      typeof method[key] === 'number' && 
      method[key] > 0
    )
    
    if (idKey) {
      return Number(method[idKey])
    }
    
    return null
  }

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="text-gray-600">Loading payment methods...</div>
      </div>
    )
  }

  return (
    <div className="max-w-4xl">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Payment Methods</h2>
        {!showForm && (
          <button
            onClick={() => {
              setShowForm(true)
              setFormData({ cardNumber: '', cardType: 'Debit', cardExpirationDate: '' })
              setError('')
            }}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors"
          >
            Add Payment Method
          </button>
        )}
      </div>

      {error && (
        <div className="mb-4 rounded-md bg-red-50 p-4">
          <div className="text-sm text-red-800">{error}</div>
        </div>
      )}

      {successMessage && (
        <div className="mb-4 rounded-md bg-green-50 p-4">
          <div className="text-sm text-green-800">{successMessage}</div>
        </div>
      )}

      {showForm && (
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Add New Payment Method
          </h3>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="cardNumber" className="block text-sm font-medium text-gray-700 mb-1">
                Card Number
              </label>
              <input
                type="text"
                id="cardNumber"
                name="cardNumber"
                value={formData.cardNumber}
                onChange={handleInputChange}
                required
                maxLength={255}
                placeholder="Enter card number"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div>
              <label htmlFor="cardType" className="block text-sm font-medium text-gray-700 mb-1">
                Card Type
              </label>
              <select
                id="cardType"
                name="cardType"
                value={formData.cardType}
                onChange={handleInputChange}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="Debit">Debit</option>
                <option value="Credit">Credit</option>
              </select>
            </div>

            <div>
              <label htmlFor="cardExpirationDate" className="block text-sm font-medium text-gray-700 mb-1">
                Expiration Date
              </label>
              <input
                type="date"
                id="cardExpirationDate"
                name="cardExpirationDate"
                value={formData.cardExpirationDate}
                onChange={handleInputChange}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div className="flex gap-3">
              <button
                type="submit"
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors"
              >
                Add Payment Method
              </button>
              <button
                type="button"
                onClick={handleCancel}
                className="px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {paymentMethods.length === 0 && !showForm ? (
        <div className="bg-white rounded-lg shadow p-8 text-center">
          <p className="text-gray-600 text-lg">No payment methods saved yet.</p>
          <p className="text-gray-500 text-sm mt-2">Add a payment method to get started.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {paymentMethods.map((method: any, index) => {
            // Extract ID using helper function
            const spmID = extractPaymentMethodId(method)
            const cardNumber = method.cardNumber ?? method.CardNumber ?? ''
            const cardType = method.cardType ?? method.CardType ?? ''
            const cardExpirationDate = method.cardExpirationDate ?? method.CardExpirationDate ?? ''
            const createdAt = method.createdAt ?? method.CreatedAt
            
            if (!spmID || isNaN(spmID)) {
              console.error('Payment method missing or invalid ID:', {
                method,
                allKeys: Object.keys(method),
                converted: spmID,
                spmIDValue: method.spmID,
                SPMIDValue: method.SPMID,
                firstKey: Object.keys(method)[0],
                firstValue: method[Object.keys(method)[0]]
              })
            }
            
            return (
              <div key={spmID ?? `payment-${index}`} className="bg-white rounded-lg shadow p-6">
                <div className="flex justify-between items-start">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <h3 className="text-lg font-semibold text-gray-900">
                        {formatCardNumber(cardNumber)}
                      </h3>
                      <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded-full text-xs font-medium">
                        {cardType}
                      </span>
                    </div>
                    <div className="text-sm text-gray-600 space-y-1">
                      {cardExpirationDate && (
                        <p>
                          <span className="font-medium">Expires:</span>{' '}
                          {formatDate(cardExpirationDate)}
                        </p>
                      )}
                      {createdAt && (
                        <p>
                          <span className="font-medium">Added:</span> {formatDate(createdAt)}
                        </p>
                      )}
                    </div>
                  </div>
                  <button
                    onClick={() => {
                      console.log('Delete clicked - spmID:', spmID, 'type:', typeof spmID, 'method object:', method)
                      if (spmID !== null && spmID !== undefined && !isNaN(spmID) && spmID > 0) {
                        handleDelete(spmID)
                      } else {
                        console.error('Cannot delete - invalid ID:', spmID)
                        setError(`Payment method ID is missing or invalid. ID: ${spmID}`)
                      }
                    }}
                    disabled={deletingId !== null && Number(deletingId) === Number(spmID)}
                    className="ml-4 px-3 py-2 text-sm bg-red-600 text-white rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    {deletingId !== null && Number(deletingId) === Number(spmID) ? 'Deleting...' : 'Delete'}
                  </button>
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}

export default PaymentMethods

