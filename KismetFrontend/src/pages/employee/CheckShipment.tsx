import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { shipmentApi, sessionApi } from '../../services/api'

interface ShipmentResponseToEmployee {
  shipmentID: number
  orderID: number
  customsDocRef: string | null
  shipmentStatus: number // 0: Pending, 1: InTransit, 2: Delivered, 3: Failed
  isLocked: boolean
  lockedAt: string | null
  shipmentDate: string | null
  originCountry: string | null
  destinationCountry: string | null
  expectedDeliveryDate: string | null
  actualDeliveryDate: string | null
  createdAt: string
  lastUpdatedAt: string | null
}

const CheckShipment = () => {
  const navigate = useNavigate()
  const [shipmentId, setShipmentId] = useState('')
  const [orderId, setOrderId] = useState('')
  const [shipment, setShipment] = useState<ShipmentResponseToEmployee | null>(null)
  const [shipments, setShipments] = useState<ShipmentResponseToEmployee[]>([])
  const [searchType, setSearchType] = useState<'shipment' | 'order'>('shipment')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const [employeeId, setEmployeeId] = useState<number | null>(null)
  const [accessLevel, setAccessLevel] = useState<number | null>(null)

  // Get employee ID and access level on component mount
  useEffect(() => {
    const loadEmployeeInfo = async () => {
      try {
        // Try to get from sessionStorage first (faster)
        const storedEmployeeId = sessionStorage.getItem('userId')
        const storedAccessLevel = sessionStorage.getItem('accessLevel')
        
        if (storedEmployeeId) {
          setEmployeeId(parseInt(storedEmployeeId))
        }
        if (storedAccessLevel) {
          const level = parseInt(storedAccessLevel)
          setAccessLevel(level)
          
          // Check access level - require at least 3
          if (level < 3) {
            navigate('/employee/dashboard', {
              state: { message: 'You do not have permission to access this page. Access Level 3 or higher is required.' }
            })
            return
          }
        }
        
        // Also verify with API call
        const userInfo = await sessionApi.getCurrentUser()
        if (userInfo.userId) {
          setEmployeeId(userInfo.userId)
          // Update sessionStorage if different
          if (storedEmployeeId !== userInfo.userId.toString()) {
            sessionStorage.setItem('userId', userInfo.userId.toString())
          }
          
          // Check and set access level
          if (userInfo.accessLevel !== undefined) {
            setAccessLevel(userInfo.accessLevel)
            if (userInfo.accessLevel < 3) {
              navigate('/employee/dashboard', {
                state: { message: 'You do not have permission to access this page. Access Level 3 or higher is required.' }
              })
              return
            }
            // Update sessionStorage if different
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setShipment(null)
    setShipments([])

    if (searchType === 'shipment') {
      if (!shipmentId.trim()) {
        setError('Please enter a shipment ID')
        return
      }

      const parsedShipmentId = parseInt(shipmentId)
      if (isNaN(parsedShipmentId) || parsedShipmentId <= 0) {
        setError('Please enter a valid shipment ID (positive number)')
        return
      }

      setIsLoading(true)
      try {
        console.log('Fetching shipment with ID:', parsedShipmentId)
        const shipmentData = await shipmentApi.getShipmentById(parsedShipmentId)
        console.log('Shipment data received:', shipmentData)
        setShipment(shipmentData)
        setError('')
      } catch (err: any) {
        console.error('Error fetching shipment:', err)
        const errorMessage = err.response?.data?.error || err.message || 'Failed to fetch shipment'
        setError(errorMessage)
        setShipment(null)
      } finally {
        setIsLoading(false)
      }
    } else {
      // Search by OrderID
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
        console.log('Fetching shipments for order ID:', parsedOrderId)
        const shipmentsData = await shipmentApi.getShipmentsByOrderId(parsedOrderId)
        console.log('Shipments data received:', shipmentsData)
        setShipments(Array.isArray(shipmentsData) ? shipmentsData : [])
        setError('')
      } catch (err: any) {
        console.error('Error fetching shipments:', err)
        const errorMessage = err.response?.data?.error || err.message || 'Failed to fetch shipments'
        setError(errorMessage)
        setShipments([])
      } finally {
        setIsLoading(false)
      }
    }
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

  const getStatusDisplay = (status: number): { text: string; color: string } => {
    switch (status) {
      case 0:
        return { text: 'Pending', color: 'bg-yellow-100 text-yellow-800' }
      case 1:
        return { text: 'In Transit', color: 'bg-blue-100 text-blue-800' }
      case 2:
        return { text: 'Delivered', color: 'bg-green-100 text-green-800' }
      case 3:
        return { text: 'Failed', color: 'bg-red-100 text-red-800' }
      default:
        return { text: 'Unknown', color: 'bg-gray-100 text-gray-800' }
    }
  }

  const renderShipmentDetails = (shipmentData: ShipmentResponseToEmployee) => {
    const status = getStatusDisplay(shipmentData.shipmentStatus)
    
    return (
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex justify-between items-center mb-6">
          <h3 className="text-xl font-semibold text-gray-800">Shipment #{shipmentData.shipmentID}</h3>
          <span className={`px-3 py-1 rounded-full text-sm font-medium ${status.color}`}>
            {status.text}
          </span>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Basic Information */}
          <div className="space-y-4">
            <h4 className="text-lg font-semibold text-gray-700 border-b pb-2">Basic Information</h4>
            <div className="space-y-2 text-gray-700">
              <p><strong>Shipment ID:</strong> {shipmentData.shipmentID}</p>
              <p><strong>Order ID:</strong> {shipmentData.orderID}</p>
              <p><strong>Customs Doc Ref:</strong> {shipmentData.customsDocRef || 'N/A'}</p>
              <p><strong>Created At:</strong> {formatDate(shipmentData.createdAt)}</p>
              {shipmentData.lastUpdatedAt && (
                <p><strong>Last Updated:</strong> {formatDate(shipmentData.lastUpdatedAt)}</p>
              )}
            </div>
          </div>

          {/* Status & Location Information */}
          <div className="space-y-4">
            <h4 className="text-lg font-semibold text-gray-700 border-b pb-2">Status & Location</h4>
            <div className="space-y-2 text-gray-700">
              <p>
                <strong>Is Locked:</strong>{' '}
                <span className={shipmentData.isLocked ? 'text-orange-600 font-semibold' : 'text-gray-600'}>
                  {shipmentData.isLocked ? 'Yes' : 'No'}
                </span>
              </p>
              {shipmentData.lockedAt && (
                <p><strong>Locked At:</strong> {formatDate(shipmentData.lockedAt)}</p>
              )}
              <p><strong>Origin Country:</strong> {shipmentData.originCountry || 'N/A'}</p>
              <p><strong>Destination Country:</strong> {shipmentData.destinationCountry || 'N/A'}</p>
            </div>
          </div>

          {/* Dates Information */}
          <div className="space-y-4">
            <h4 className="text-lg font-semibold text-gray-700 border-b pb-2">Dates</h4>
            <div className="space-y-2 text-gray-700">
              <p><strong>Shipment Date:</strong> {formatDate(shipmentData.shipmentDate)}</p>
              <p><strong>Expected Delivery Date:</strong> {formatDate(shipmentData.expectedDeliveryDate)}</p>
              <p><strong>Actual Delivery Date:</strong> {formatDate(shipmentData.actualDeliveryDate)}</p>
            </div>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="max-w-4xl mx-auto">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Check Shipment</h2>

      {/* Search Form */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Search Type Toggle */}
          <div className="flex gap-4 mb-4">
            <label className="flex items-center">
              <input
                type="radio"
                name="searchType"
                value="shipment"
                checked={searchType === 'shipment'}
                onChange={(e) => {
                  setSearchType('shipment')
                  setShipment(null)
                  setShipments([])
                  setError('')
                }}
                className="mr-2"
              />
              <span className="text-sm font-medium text-gray-700">Search by Shipment ID</span>
            </label>
            <label className="flex items-center">
              <input
                type="radio"
                name="searchType"
                value="order"
                checked={searchType === 'order'}
                onChange={(e) => {
                  setSearchType('order')
                  setShipment(null)
                  setShipments([])
                  setError('')
                }}
                className="mr-2"
              />
              <span className="text-sm font-medium text-gray-700">Search by Order ID</span>
            </label>
          </div>

          <div className="flex gap-4">
            {searchType === 'shipment' ? (
              <div className="flex-1">
                <label htmlFor="shipmentId" className="block text-sm font-medium text-gray-700 mb-2">
                  Shipment ID
                </label>
                <input
                  id="shipmentId"
                  type="text"
                  value={shipmentId}
                  onChange={(e) => setShipmentId(e.target.value)}
                  placeholder="Enter shipment ID"
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  disabled={isLoading}
                />
              </div>
            ) : (
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
            )}
            <div className="flex items-end">
              <button
                type="submit"
                disabled={isLoading}
                className="px-6 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {isLoading ? 'Searching...' : 'Search'}
              </button>
            </div>
          </div>
        </form>
      </div>

      {/* Loading Indicator */}
      {isLoading && (
        <div className="bg-blue-50 border border-blue-200 rounded-md p-4 mb-6">
          <div className="text-sm text-blue-800">Searching for shipment(s)...</div>
        </div>
      )}

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-md p-4 mb-6">
          <div className="text-sm font-medium text-red-800">Error: {error}</div>
        </div>
      )}

      {/* Single Shipment Details */}
      {shipment && renderShipmentDetails(shipment)}

      {/* Multiple Shipments (from OrderID search) */}
      {shipments.length > 0 && (
        <div className="space-y-4">
          <h3 className="text-lg font-semibold text-gray-800">
            Found {shipments.length} shipment(s) for Order #{orderId}
          </h3>
          {shipments.map((shipmentData) => {
            // Handle both camelCase and PascalCase property names
            const id = (shipmentData as any).shipmentID || (shipmentData as any).ShipmentID || shipmentData.shipmentID
            return (
              <div key={id}>
                {renderShipmentDetails(shipmentData)}
              </div>
            )
          })}
        </div>
      )}

      {/* Empty State */}
      {!shipment && shipments.length === 0 && !error && !isLoading && (
        <div className="bg-white rounded-lg shadow-md p-12 text-center">
          <p className="text-gray-500 text-lg">
            {searchType === 'shipment' 
              ? 'Enter a shipment ID above to view shipment details'
              : 'Enter an order ID above to view shipments for that order'}
          </p>
        </div>
      )}
    </div>
  )
}

export default CheckShipment

