import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { viewsApi, sessionApi } from '../../services/api'

interface OrderDetailsView {
  orderID: number
  orderNumber: string
  orderType: string
  orderStatus: number
  totalAmount: number
  orderDate: string
  customerID: number
  customerName: string
  customerEmail: string
  city?: string
  country?: string
  approvedByEmployeeName?: string
  approvalDate?: string
  totalBatches: number
  totalQuantity: number
  shipmentStatusSummary: string
  totalShipments: number
}

interface ShipmentTrackingView {
  shipmentID: number
  orderID: number
  orderNumber: string
  shipmentStatusText: string
  shipmentDate?: string
  expectedDeliveryDate?: string
  actualDeliveryDate?: string
  customerID: number
  customerName: string
  customerCity?: string
  totalBatches: number
  totalUnits: number
  totalValue: number
  deliveryDelayDays?: number
  daysInTransit?: number
}

interface FinancialOverviewView {
  fTransactionID: number
  transactionType: string
  totalAmount: number
  totalPaid: number
  remainingBalance: number
  paymentStatusText: string
  transactionDate: string
  customerID: number
  customerName: string
  orderID: number
  orderNumber: string
  invoiceNumber: string
  treasuryAmount?: number
  treasuryBalanceAfter?: number
  daysUntilDue: number
  transactionStatus: string
  paymentPercentage: number
}

interface InventoryStatusView {
  fabricID: number
  fabricType: string
  color?: string
  currentStock: number
  totalBatchesProduced: number
  unshippedBatches: number
  shippedBatches: number
  totalQuantityUsed: number
  unshippedQuantity: number
  shippedQuantity: number
  totalRevenue: number
  averageBatchPrice: number
  totalOrders: number
  latestProductionDate?: string
  stockStatus: string
  avgQuantityPerBatch: number
}

const Views = () => {
  const navigate = useNavigate()
  const [accessLevel, setAccessLevel] = useState<number | null>(null)
  const [activeTab, setActiveTab] = useState<'customer' | 'fabric'>('customer')
  
  // Customer search
  const [customerId, setCustomerId] = useState('')
  const [orderDetails, setOrderDetails] = useState<OrderDetailsView[]>([])
  const [shipmentTracking, setShipmentTracking] = useState<ShipmentTrackingView[]>([])
  const [financialOverview, setFinancialOverview] = useState<FinancialOverviewView[]>([])
  
  // Fabric search
  const [fabricId, setFabricId] = useState('')
  const [inventoryStatus, setInventoryStatus] = useState<InventoryStatusView | null>(null)
  
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    const checkAccess = async () => {
      const storedAccessLevel = sessionStorage.getItem('accessLevel')
      const parsedAccessLevel = storedAccessLevel ? parseInt(storedAccessLevel) : null

      if (parsedAccessLevel === null || parsedAccessLevel < 4) {
        navigate('/employee/dashboard', { state: { error: 'You do not have sufficient access to view this page.' } })
        return
      }
      setAccessLevel(parsedAccessLevel)
    }
    checkAccess()
  }, [navigate])

  const handleCustomerSearch = async () => {
    if (!customerId || isNaN(Number(customerId))) {
      setError('Please enter a valid Customer ID')
      return
    }

    setIsLoading(true)
    setError('')
    setOrderDetails([])
    setShipmentTracking([])
    setFinancialOverview([])

    try {
      const [orders, shipments, financial] = await Promise.all([
        viewsApi.getOrderDetails(Number(customerId)),
        viewsApi.getShipmentTracking(Number(customerId)),
        viewsApi.getFinancialOverview(Number(customerId))
      ])

      setOrderDetails(Array.isArray(orders) ? orders : [])
      setShipmentTracking(Array.isArray(shipments) ? shipments : [])
      setFinancialOverview(Array.isArray(financial) ? financial : [])
    } catch (err: any) {
      console.error('Error loading customer views:', err)
      setError(err.response?.data?.error || err.message || 'Failed to load customer views')
    } finally {
      setIsLoading(false)
    }
  }

  const handleFabricSearch = async () => {
    if (!fabricId || isNaN(Number(fabricId))) {
      setError('Please enter a valid Fabric ID')
      return
    }

    setIsLoading(true)
    setError('')
    setInventoryStatus(null)

    try {
      const inventory = await viewsApi.getInventoryStatus(Number(fabricId))
      setInventoryStatus(inventory)
    } catch (err: any) {
      console.error('Error loading inventory status:', err)
      if (err.response?.status === 404) {
        setError('Fabric not found')
      } else {
        setError(err.response?.data?.error || err.message || 'Failed to load inventory status')
      }
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

  const formatDate = (dateString: string | null | undefined) => {
    if (!dateString) return 'N/A'
    return new Date(dateString).toLocaleString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  const getOrderStatusText = (status: number): string => {
    switch (status) {
      case 0: return 'Pending'
      case 1: return 'Approved'
      case 2: return 'Shipped'
      case 3: return 'Delivered'
      case 4: return 'Cancelled'
      default: return 'Unknown'
    }
  }

  if (accessLevel === null || accessLevel < 4) {
    return null
  }

  return (
    <div className="max-w-7xl mx-auto">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Views</h2>

      {/* Tabs */}
      <div className="mb-6 border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab('customer')}
            className={`py-4 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'customer'
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Customer Views
          </button>
          <button
            onClick={() => setActiveTab('fabric')}
            className={`py-4 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'fabric'
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Inventory Status
          </button>
        </nav>
      </div>

      {/* Error Message */}
      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-4">
          <div className="text-sm font-medium text-red-800">Error: {error}</div>
        </div>
      )}

      {/* Customer Views */}
      {activeTab === 'customer' && (
        <div className="space-y-6">
          {/* Search */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">Search by Customer ID</h3>
            <div className="flex gap-4">
              <input
                type="number"
                value={customerId}
                onChange={(e) => setCustomerId(e.target.value)}
                placeholder="Enter Customer ID"
                className="flex-1 px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
              <button
                onClick={handleCustomerSearch}
                disabled={isLoading}
                className="px-6 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLoading ? 'Loading...' : 'Search'}
              </button>
            </div>
          </div>

          {/* Order Details */}
          {orderDetails.length > 0 && (
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-semibold text-gray-800 mb-4">Order Details</h3>
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Order #</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Type</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Amount</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Batches</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Shipments</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date</th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {orderDetails.map((order) => (
                      <tr key={order.orderID}>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{order.orderNumber}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{order.orderType}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{getOrderStatusText(order.orderStatus)}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{formatCurrency(order.totalAmount)}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{order.totalBatches}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{order.totalShipments}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{formatDate(order.orderDate)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Shipment Tracking */}
          {shipmentTracking.length > 0 && (
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-semibold text-gray-800 mb-4">Shipment Tracking</h3>
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Shipment ID</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Order #</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Batches</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Value</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Expected Delivery</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Days in Transit</th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {shipmentTracking.map((shipment) => (
                      <tr key={shipment.shipmentID}>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{shipment.shipmentID}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{shipment.orderNumber}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{shipment.shipmentStatusText}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{shipment.totalBatches}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{formatCurrency(shipment.totalValue)}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{formatDate(shipment.expectedDeliveryDate)}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{shipment.daysInTransit ?? 'N/A'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Financial Overview */}
          {financialOverview.length > 0 && (
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-semibold text-gray-800 mb-4">Financial Overview</h3>
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Transaction ID</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Type</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Total Amount</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Paid</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Remaining</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Payment %</th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {financialOverview.map((financial) => (
                      <tr key={financial.fTransactionID}>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{financial.fTransactionID}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{financial.transactionType}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{formatCurrency(financial.totalAmount)}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-green-600">{formatCurrency(financial.totalPaid)}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{formatCurrency(financial.remainingBalance)}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{financial.paymentStatusText}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{financial.paymentPercentage.toFixed(1)}%</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Inventory Status */}
      {activeTab === 'fabric' && (
        <div className="space-y-6">
          {/* Search */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">Search by Fabric ID</h3>
            <div className="flex gap-4">
              <input
                type="number"
                value={fabricId}
                onChange={(e) => setFabricId(e.target.value)}
                placeholder="Enter Fabric ID"
                className="flex-1 px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
              <button
                onClick={handleFabricSearch}
                disabled={isLoading}
                className="px-6 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLoading ? 'Loading...' : 'Search'}
              </button>
            </div>
          </div>

          {/* Inventory Status Details */}
          {inventoryStatus && (
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-semibold text-gray-800 mb-4">Inventory & Production Status</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <h4 className="font-medium text-gray-700 mb-3">Fabric Information</h4>
                  <div className="space-y-2 text-sm">
                    <p><span className="font-medium">Fabric ID:</span> {inventoryStatus.fabricID}</p>
                    <p><span className="font-medium">Type:</span> {inventoryStatus.fabricType}</p>
                    {inventoryStatus.color && <p><span className="font-medium">Color:</span> {inventoryStatus.color}</p>}
                    <p><span className="font-medium">Current Stock:</span> {inventoryStatus.currentStock}</p>
                    <p><span className="font-medium">Stock Status:</span> {inventoryStatus.stockStatus}</p>
                  </div>
                </div>
                <div>
                  <h4 className="font-medium text-gray-700 mb-3">Production Statistics</h4>
                  <div className="space-y-2 text-sm">
                    <p><span className="font-medium">Total Batches:</span> {inventoryStatus.totalBatchesProduced}</p>
                    <p><span className="font-medium">Shipped Batches:</span> {inventoryStatus.shippedBatches}</p>
                    <p><span className="font-medium">Unshipped Batches:</span> {inventoryStatus.unshippedBatches}</p>
                    <p><span className="font-medium">Total Quantity Used:</span> {inventoryStatus.totalQuantityUsed}</p>
                    <p><span className="font-medium">Avg Quantity/Batch:</span> {inventoryStatus.avgQuantityPerBatch.toFixed(2)}</p>
                  </div>
                </div>
                <div>
                  <h4 className="font-medium text-gray-700 mb-3">Financial Information</h4>
                  <div className="space-y-2 text-sm">
                    <p><span className="font-medium">Total Revenue:</span> {formatCurrency(inventoryStatus.totalRevenue)}</p>
                    <p><span className="font-medium">Average Batch Price:</span> {formatCurrency(inventoryStatus.averageBatchPrice)}</p>
                    <p><span className="font-medium">Total Orders:</span> {inventoryStatus.totalOrders}</p>
                  </div>
                </div>
                <div>
                  <h4 className="font-medium text-gray-700 mb-3">Additional Information</h4>
                  <div className="space-y-2 text-sm">
                    <p><span className="font-medium">Shipped Quantity:</span> {inventoryStatus.shippedQuantity}</p>
                    <p><span className="font-medium">Unshipped Quantity:</span> {inventoryStatus.unshippedQuantity}</p>
                    {inventoryStatus.latestProductionDate && (
                      <p><span className="font-medium">Latest Production:</span> {formatDate(inventoryStatus.latestProductionDate)}</p>
                    )}
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

export default Views

