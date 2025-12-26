import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { paymentApi, sessionApi } from '../../services/api'

interface Payment {
  paymentID: number
  fTransactionID: number
  paymentAmount: number
  paymentType: string
  paymentDate: string
  paymentMethod: string | null
  referenceNumber: string | null
}

const ManagePayment = () => {
  const navigate = useNavigate()
  const [employeeId, setEmployeeId] = useState<number | null>(null)
  const [accessLevel, setAccessLevel] = useState<number | null>(null)
  const [searchType, setSearchType] = useState<'payment' | 'customer'>('payment')
  const [paymentId, setPaymentId] = useState('')
  const [customerId, setCustomerId] = useState('')
  const [payment, setPayment] = useState<Payment | null>(null)
  const [payments, setPayments] = useState<Payment[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    const loadEmployeeInfo = async () => {
      try {
        const storedEmployeeId = sessionStorage.getItem('userId')
        const storedAccessLevel = sessionStorage.getItem('accessLevel')
        
        if (storedEmployeeId) {
          setEmployeeId(parseInt(storedEmployeeId))
        }
        if (storedAccessLevel) {
          const level = parseInt(storedAccessLevel)
          setAccessLevel(level)
          
          if (level < 4) {
            navigate('/employee/dashboard', {
              state: { message: 'You do not have permission to access this page. Access Level 4 or higher is required.' }
            })
            return
          }
        }
        
        const userInfo = await sessionApi.getCurrentUser()
        if (userInfo.userId) {
          setEmployeeId(userInfo.userId)
          if (storedEmployeeId !== userInfo.userId.toString()) {
            sessionStorage.setItem('userId', userInfo.userId.toString())
          }
          
          if (userInfo.accessLevel !== undefined) {
            setAccessLevel(userInfo.accessLevel)
            if (userInfo.accessLevel < 4) {
              navigate('/employee/dashboard', {
                state: { message: 'You do not have permission to access this page. Access Level 4 or higher is required.' }
              })
              return
            }
            if (storedAccessLevel !== userInfo.accessLevel.toString()) {
              sessionStorage.setItem('accessLevel', userInfo.accessLevel.toString())
            }
          }
        }
      } catch (err) {
        console.error('Failed to get employee info:', err)
        navigate('/employee/dashboard')
      }
    }
    loadEmployeeInfo()
  }, [navigate])

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setPayment(null)
    setPayments([])
    setIsLoading(true)

    try {
      if (searchType === 'payment') {
        if (!paymentId.trim()) {
          setError('Please enter a Payment ID')
          setIsLoading(false)
          return
        }

        const parsedId = parseInt(paymentId)
        if (isNaN(parsedId) || parsedId <= 0) {
          setError('Please enter a valid Payment ID')
          setIsLoading(false)
          return
        }

        const data = await paymentApi.getById(parsedId)
        setPayment(data)
      } else {
        if (!customerId.trim()) {
          setError('Please enter a Customer ID')
          setIsLoading(false)
          return
        }

        const parsedId = parseInt(customerId)
        if (isNaN(parsedId) || parsedId <= 0) {
          setError('Please enter a valid Customer ID')
          setIsLoading(false)
          return
        }

        const data = await paymentApi.getByCustomerId(parsedId)
        setPayments(Array.isArray(data) ? data : [])
      }
    } catch (err: any) {
      console.error('Error searching:', err)
      const errorMessage = err.response?.data?.error || err.message || 'Failed to search'
      setError(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('tr-TR', {
      style: 'currency',
      currency: 'TRY',
    }).format(amount)
  }

  const formatDate = (dateString: string | null) => {
    if (!dateString) return 'N/A'
    return new Date(dateString).toLocaleString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  const renderPaymentDetails = (paymentData: Payment) => {
    return (
      <div className="bg-white rounded-lg shadow-md p-6">
        <h3 className="text-xl font-semibold text-gray-800 mb-6">Payment #{paymentData.paymentID}</h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-4">
            <h4 className="text-lg font-semibold text-gray-700 border-b pb-2">Payment Information</h4>
            <div className="space-y-2 text-gray-700">
              <p><strong>Payment ID:</strong> {paymentData.paymentID}</p>
              <p><strong>Financial Transaction ID:</strong> {paymentData.fTransactionID}</p>
              <p><strong>Payment Type:</strong> {paymentData.paymentType}</p>
              <p><strong>Payment Date:</strong> {formatDate(paymentData.paymentDate)}</p>
            </div>
          </div>

          <div className="space-y-4">
            <h4 className="text-lg font-semibold text-gray-700 border-b pb-2">Payment Details</h4>
            <div className="space-y-2 text-gray-700">
              <p><strong>Payment Amount:</strong> {formatCurrency(paymentData.paymentAmount)}</p>
              <p><strong>Payment Method:</strong> {paymentData.paymentMethod || 'N/A'}</p>
              <p><strong>Reference Number:</strong> {paymentData.referenceNumber || 'N/A'}</p>
            </div>
          </div>
        </div>
      </div>
    )
  }

  if (accessLevel === null || accessLevel < 4) {
    return (
      <div className="max-w-4xl mx-auto">
        <div className="bg-yellow-50 border border-yellow-200 rounded-md p-4">
          <div className="text-sm text-yellow-800">Loading access level...</div>
        </div>
      </div>
    )
  }

  return (
    <div className="max-w-6xl mx-auto">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Manage Payment</h2>

      {/* Search Form */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <form onSubmit={handleSearch} className="space-y-4">
          <div className="flex gap-4 mb-4">
            <label className="flex items-center">
              <input
                type="radio"
                name="searchType"
                value="payment"
                checked={searchType === 'payment'}
                onChange={(e) => {
                  setSearchType('payment')
                  setPayment(null)
                  setPayments([])
                  setError('')
                }}
                className="mr-2"
              />
              <span className="text-sm font-medium text-gray-700">Search by Payment ID</span>
            </label>
            <label className="flex items-center">
              <input
                type="radio"
                name="searchType"
                value="customer"
                checked={searchType === 'customer'}
                onChange={(e) => {
                  setSearchType('customer')
                  setPayment(null)
                  setPayments([])
                  setError('')
                }}
                className="mr-2"
              />
              <span className="text-sm font-medium text-gray-700">Search by Customer ID</span>
            </label>
          </div>

          <div className="flex gap-4">
            {searchType === 'payment' ? (
              <div className="flex-1">
                <label htmlFor="paymentId" className="block text-sm font-medium text-gray-700 mb-2">
                  Payment ID
                </label>
                <input
                  id="paymentId"
                  type="text"
                  value={paymentId}
                  onChange={(e) => setPaymentId(e.target.value)}
                  placeholder="Enter payment ID"
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                  disabled={isLoading}
                />
              </div>
            ) : (
              <div className="flex-1">
                <label htmlFor="customerId" className="block text-sm font-medium text-gray-700 mb-2">
                  Customer ID
                </label>
                <input
                  id="customerId"
                  type="text"
                  value={customerId}
                  onChange={(e) => setCustomerId(e.target.value)}
                  placeholder="Enter customer ID"
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                  disabled={isLoading}
                />
              </div>
            )}
            <div className="flex items-end">
              <button
                type="submit"
                disabled={isLoading}
                className="px-6 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLoading ? 'Searching...' : 'Search'}
              </button>
            </div>
          </div>
        </form>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-md p-4 mb-6">
          <div className="text-sm font-medium text-red-800">Error: {error}</div>
        </div>
      )}

      {/* Single Payment */}
      {payment && renderPaymentDetails(payment)}

      {/* Multiple Payments */}
      {payments.length > 0 && (
        <div className="space-y-4">
          <h3 className="text-lg font-semibold text-gray-800">
            Found {payments.length} payment(s) for Customer #{customerId}
          </h3>
          {payments.map((paymentData) => (
            <div key={paymentData.paymentID}>
              {renderPaymentDetails(paymentData)}
            </div>
          ))}
        </div>
      )}

      {!payment && payments.length === 0 && !error && !isLoading && (
        <div className="bg-white rounded-lg shadow-md p-12 text-center">
          <p className="text-gray-500 text-lg">
            {searchType === 'payment' 
              ? 'Enter a Payment ID above to view details'
              : 'Enter a Customer ID above to view payments'}
          </p>
        </div>
      )}
    </div>
  )
}

export default ManagePayment

