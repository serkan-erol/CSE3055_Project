import { useState, useEffect } from 'react'
import { orderApi, sessionApi } from '../../services/api'

interface OrderResponseToEmployee {
  orderID: number
  customerID: number
  reliabilityStatus: boolean
  orderNumber: string
  orderType: string
  totalAmount: number
  orderStatus: number // 0: Pending, 1: Approved, 2: Shipped, 3: Delivered, 4: Cancelled
  isApproved: boolean
  approvedBy: number | null
  approvalDate: string | null
  isLocked: boolean
  lockedAt: string | null
  orderDate: string
  lastUpdatedAt: string | null
}

const CheckOrder = () => {
  const [orderId, setOrderId] = useState('')
  const [order, setOrder] = useState<OrderResponseToEmployee | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const [isApproving, setIsApproving] = useState(false)
  const [approveError, setApproveError] = useState('')
  const [approveSuccess, setApproveSuccess] = useState('')
  const [employeeId, setEmployeeId] = useState<number | null>(null)

  // Get employee ID on component mount
  useEffect(() => {
    const loadEmployeeId = async () => {
      try {
        const userInfo = await sessionApi.getCurrentUser()
        if (userInfo.userId) {
          setEmployeeId(userInfo.userId)
        }
      } catch (err) {
        console.error('Failed to get employee ID:', err)
      }
    }
    loadEmployeeId()
  }, [])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setOrder(null)

    if (!orderId.trim()) {
      setError('Please enter an order ID')
      return
    }

    const parsedOrderId = parseInt(orderId)
    if (isNaN(parsedOrderId) || parsedOrderId <= 0) {
      setError('Please enter a valid order ID (positive number)')
      return
    }

    setIsLoading(true)
    try {
      console.log('Fetching order with ID:', parsedOrderId)
      const orderData = await orderApi.getByIdForEmployee(parsedOrderId)
      console.log('Order data received:', orderData)
      setOrder(orderData)
      setError('')
    } catch (err: any) {
      console.error('Error fetching order:', err)
      const errorMessage = err.response?.data?.error || err.message || 'Failed to fetch order'
      setError(errorMessage)
      setOrder(null)
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

  const handleApprove = async () => {
    if (!order || !employeeId) {
      setApproveError('Unable to approve order. Please refresh the page.')
      return
    }

    // Check if order is already approved
    if (order.isApproved) {
      setApproveError('This order is already approved.')
      return
    }

    // Check if order status is pending
    if (order.orderStatus !== 0) {
      setApproveError('Only pending orders can be approved.')
      return
    }

    setIsApproving(true)
    setApproveError('')
    setApproveSuccess('')

    try {
      const approvedOrder = await orderApi.approveOrder(employeeId, order.orderID)
      setOrder(approvedOrder)
      setApproveSuccess('Order approved successfully!')
      setApproveError('')
      // Clear success message after 5 seconds
      setTimeout(() => setApproveSuccess(''), 5000)
    } catch (err: any) {
      console.error('Error approving order:', err)
      const errorMessage = err.response?.data?.error || err.message || 'Failed to approve order'
      setApproveError(errorMessage)
      setApproveSuccess('')
    } finally {
      setIsApproving(false)
    }
  }

  const getStatusDisplay = (status: number): { text: string; color: string } => {
    switch (status) {
      case 0:
        return { text: 'Pending', color: 'bg-yellow-100 text-yellow-800' }
      case 1:
        return { text: 'Approved', color: 'bg-blue-100 text-blue-800' }
      case 2:
        return { text: 'Shipped', color: 'bg-purple-100 text-purple-800' }
      case 3:
        return { text: 'Delivered', color: 'bg-green-100 text-green-800' }
      case 4:
        return { text: 'Cancelled', color: 'bg-red-100 text-red-800' }
      default:
        return { text: 'Unknown', color: 'bg-gray-100 text-gray-800' }
    }
  }

  return (
    <div className="max-w-4xl mx-auto">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Check Order</h2>

      {/* Search Form */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <form onSubmit={handleSubmit} className="flex gap-4">
          <div className="flex-1">
            <label htmlFor="orderId" className="block text-sm font-medium text-gray-700 mb-2">
              Order ID
            </label>
            <input
              id="orderId"
              type="text"
              value={orderId}
              onChange={(e) => setOrderId(e.target.value)}
              placeholder="Enter order ID"
              className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              disabled={isLoading}
            />
          </div>
          <div className="flex items-end">
            <button
              type="submit"
              disabled={isLoading}
              className="px-6 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {isLoading ? 'Searching...' : 'Search'}
            </button>
          </div>
        </form>
      </div>

      {/* Loading Indicator */}
      {isLoading && (
        <div className="bg-blue-50 border border-blue-200 rounded-md p-4 mb-6">
          <div className="text-sm text-blue-800">Searching for order...</div>
        </div>
      )}

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-md p-4 mb-6">
          <div className="text-sm font-medium text-red-800">Error: {error}</div>
        </div>
      )}

      {/* Order Details */}
      {order && (
        <div className="bg-white rounded-lg shadow-md p-6">
          <div className="flex justify-between items-center mb-6">
            <h3 className="text-xl font-semibold text-gray-800">Order #{order.orderNumber}</h3>
            <div className="flex items-center gap-4">
              {(() => {
                const status = getStatusDisplay(order.orderStatus)
                return (
                  <span className={`px-3 py-1 rounded-full text-sm font-medium ${status.color}`}>
                    {status.text}
                  </span>
                )
              })()}
              {/* Approve Button - Only show if order is pending and not approved */}
              {order.orderStatus === 0 && !order.isApproved && (
                <button
                  onClick={handleApprove}
                  disabled={isApproving || !employeeId}
                  className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors text-sm font-medium"
                >
                  {isApproving ? 'Approving...' : 'Approve Order'}
                </button>
              )}
            </div>
          </div>

          {/* Approval Success Message */}
          {approveSuccess && (
            <div className="mb-4 bg-green-50 border border-green-200 rounded-md p-4">
              <div className="text-sm font-medium text-green-800">{approveSuccess}</div>
            </div>
          )}

          {/* Approval Error Message */}
          {approveError && (
            <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-4">
              <div className="text-sm font-medium text-red-800">Error: {approveError}</div>
            </div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Basic Information */}
            <div className="space-y-4">
              <h4 className="text-lg font-semibold text-gray-700 border-b pb-2">Basic Information</h4>
              <div className="space-y-2 text-gray-700">
                <p><strong>Order ID:</strong> {order.orderID}</p>
                <p><strong>Customer ID:</strong> {order.customerID}</p>
                <p><strong>Order Type:</strong> {order.orderType}</p>
                <p><strong>Total Amount:</strong> {formatCurrency(order.totalAmount)}</p>
                <p><strong>Order Date:</strong> {formatDate(order.orderDate)}</p>
                {order.lastUpdatedAt && (
                  <p><strong>Last Updated:</strong> {formatDate(order.lastUpdatedAt)}</p>
                )}
              </div>
            </div>

            {/* Status & Approval Information */}
            <div className="space-y-4">
              <h4 className="text-lg font-semibold text-gray-700 border-b pb-2">Status & Approval</h4>
              <div className="space-y-2 text-gray-700">
                <p>
                  <strong>Reliability Status:</strong>{' '}
                  <span className={order.reliabilityStatus ? 'text-green-600 font-semibold' : 'text-red-600 font-semibold'}>
                    {order.reliabilityStatus ? 'Reliable' : 'Unreliable'}
                  </span>
                </p>
                <p>
                  <strong>Is Approved:</strong>{' '}
                  <span className={order.isApproved ? 'text-green-600 font-semibold' : 'text-gray-600'}>
                    {order.isApproved ? 'Yes' : 'No'}
                  </span>
                </p>
                {order.approvedBy && (
                  <p><strong>Approved By:</strong> Employee #{order.approvedBy}</p>
                )}
                {order.approvalDate && (
                  <p><strong>Approval Date:</strong> {formatDate(order.approvalDate)}</p>
                )}
                <p>
                  <strong>Is Locked:</strong>{' '}
                  <span className={order.isLocked ? 'text-orange-600 font-semibold' : 'text-gray-600'}>
                    {order.isLocked ? 'Yes' : 'No'}
                  </span>
                </p>
                {order.lockedAt && (
                  <p><strong>Locked At:</strong> {formatDate(order.lockedAt)}</p>
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Empty State */}
      {!order && !error && !isLoading && (
        <div className="bg-white rounded-lg shadow-md p-12 text-center">
          <p className="text-gray-500 text-lg">Enter an order ID above to view order details</p>
        </div>
      )}
    </div>
  )
}

export default CheckOrder

