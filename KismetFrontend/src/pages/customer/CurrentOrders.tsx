import { useState, useEffect } from 'react'
import { orderApi, sessionApi } from '../../services/api'

interface Order {
  orderID: number
  customerID: number
  orderNumber: string
  orderType: string
  totalAmount: number
  orderStatus: number // 0: Pending, 1: Approved, 2: Shipped, 3: Delivered, 4: Cancelled
  orderDate: string
  lastUpdatedAt?: string
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

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount)
}

const CurrentOrders = () => {
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    loadOrders()
  }, [])

  const loadOrders = async () => {
    try {
      setLoading(true)
      setError('')
      
      // Get current user to get customerId (which equals userId for customers)
      const currentUser = await sessionApi.getCurrentUser()
      if (currentUser.userId) {
        // For customers, CustomerID equals UserID
        const customerId = currentUser.userId
        const ordersData = await orderApi.getAllForCustomer(customerId)
        setOrders(Array.isArray(ordersData) ? ordersData : [])
      }
    } catch (err: any) {
      if (err.response?.status === 404 && err.response?.data?.error === 'No orders found') {
        setOrders([])
        setError('')
      } else {
        setError(err.response?.data?.error || 'Failed to load orders')
      }
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="text-gray-600">Loading orders...</div>
      </div>
    )
  }

  return (
    <div className="max-w-7xl">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">My Orders</h2>

      {error && (
        <div className="mb-4 rounded-md bg-red-50 p-4">
          <div className="text-sm text-red-800">{error}</div>
        </div>
      )}

      {orders.length === 0 && !error ? (
        <div className="bg-white rounded-lg shadow p-8 text-center">
          <p className="text-gray-600 text-lg">You don't have any orders yet.</p>
          <p className="text-gray-500 text-sm mt-2">Start by purchasing or supplying fabric!</p>
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map((order) => {
            const status = getStatusDisplay(order.orderStatus)
            return (
              <div
                key={order.orderID}
                className="bg-white rounded-lg shadow p-6 hover:shadow-md transition-shadow"
              >
                <div className="flex flex-col md:flex-row md:items-center md:justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-4 mb-3">
                      <h3 className="text-lg font-semibold text-gray-900">
                        Order #{order.orderNumber}
                      </h3>
                      <span
                        className={`px-3 py-1 rounded-full text-xs font-medium ${status.color}`}
                      >
                        {status.text}
                      </span>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm text-gray-600">
                      <div>
                        <span className="font-medium text-gray-700">Order Type:</span>{' '}
                        {order.orderType}
                      </div>
                      <div>
                        <span className="font-medium text-gray-700">Total Amount:</span>{' '}
                        <span className="font-semibold text-gray-900">
                          {formatCurrency(order.totalAmount)}
                        </span>
                      </div>
                      <div>
                        <span className="font-medium text-gray-700">Order Date:</span>{' '}
                        {formatDate(order.orderDate)}
                      </div>
                      {order.lastUpdatedAt && (
                        <div>
                          <span className="font-medium text-gray-700">Last Updated:</span>{' '}
                          {formatDate(order.lastUpdatedAt)}
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}

export default CurrentOrders
