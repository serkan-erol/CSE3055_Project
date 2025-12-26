import { useState, useEffect } from 'react'
import { fabricApi, orderApi, sessionApi } from '../../services/api'

interface Fabric {
  fabricID: number
  fabricType: string
  composition?: string
  color?: string
  weightPerUnit?: number
  stockQuantity: number
  description?: string
}

interface CartItem {
  fabricID: number
  fabricType: string
  quantity: number
}

const PurchaseFabric = () => {
  const [fabrics, setFabrics] = useState<Fabric[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [cart, setCart] = useState<Map<number, CartItem>>(new Map())
  const [quantities, setQuantities] = useState<Map<number, number>>(new Map())
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [successMessage, setSuccessMessage] = useState('')

  useEffect(() => {
    loadFabrics()
  }, [])

  const loadFabrics = async () => {
    try {
      setLoading(true)
      setError('')
      const fabricsData = await fabricApi.getAll()
      setFabrics(Array.isArray(fabricsData) ? fabricsData : [])
    } catch (err: any) {
      setError(err.response?.data?.error || err.message || 'Failed to load fabrics')
    } finally {
      setLoading(false)
    }
  }

  const getStockStatus = (stockQuantity: number) => {
    if (stockQuantity === 0) {
      return { text: 'Out of Stock', color: 'bg-red-100 text-red-800' }
    } else if (stockQuantity < 10) {
      return { text: 'Low Stock', color: 'bg-yellow-100 text-yellow-800' }
    } else {
      return { text: 'In Stock', color: 'bg-green-100 text-green-800' }
    }
  }

  const handleQuantityChange = (fabricID: number, quantity: number) => {
    const numQuantity = parseInt(quantity.toString()) || 0
    const fabric = fabrics.find(f => f.fabricID === fabricID)
    
    if (!fabric) return

    // Validate quantity doesn't exceed stock
    if (numQuantity > fabric.stockQuantity) {
      setError(`Cannot order more than ${fabric.stockQuantity} rolls of ${fabric.fabricType}`)
      return
    }

    setError('')
    setSuccessMessage('')

    if (numQuantity > 0) {
      const newQuantities = new Map(quantities)
      newQuantities.set(fabricID, numQuantity)
      setQuantities(newQuantities)

      const newCart = new Map(cart)
      newCart.set(fabricID, {
        fabricID,
        fabricType: fabric.fabricType,
        quantity: numQuantity,
      })
      setCart(newCart)
    } else {
      const newQuantities = new Map(quantities)
      newQuantities.delete(fabricID)
      setQuantities(newQuantities)

      const newCart = new Map(cart)
      newCart.delete(fabricID)
      setCart(newCart)
    }
  }

  const handleCreateOrder = async () => {
    if (cart.size === 0) {
      setError('Please add at least one fabric to your order')
      return
    }

    try {
      setIsSubmitting(true)
      setError('')
      setSuccessMessage('')

      // Get current user to get customerId
      const currentUser = await sessionApi.getCurrentUser()
      if (!currentUser.userId) {
        setError('Unable to identify customer. Please log in again.')
        return
      }

      const customerId = currentUser.userId

      // Prepare order data
      const fabricIDs: number[] = []
      const quantitiesList: number[] = []

      cart.forEach((item) => {
        fabricIDs.push(item.fabricID)
        quantitiesList.push(item.quantity)
      })

      const orderData = {
        orderType: 'Purchase',
        fabricIDs,
        quantities: quantitiesList,
      }

      // Create the order
      const order = await orderApi.createOrder(customerId, orderData)

      // Clear cart and quantities
      setCart(new Map())
      setQuantities(new Map())
      setSuccessMessage(`Order created successfully! Order #${order.orderNumber}`)

      // Reload fabrics to update stock quantities
      await loadFabrics()
    } catch (err: any) {
      setError(err.response?.data?.error || err.message || 'Failed to create order')
    } finally {
      setIsSubmitting(false)
    }
  }

  const getCartItems = () => {
    return Array.from(cart.values())
  }

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="text-gray-600">Loading fabrics...</div>
      </div>
    )
  }

  const cartItems = getCartItems()

  return (
    <div className="max-w-7xl">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Purchase Fabric</h2>
        {cartItems.length > 0 && (
          <div className="text-sm text-gray-600">
            {cartItems.length} item{cartItems.length !== 1 ? 's' : ''} in cart
          </div>
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

      {fabrics.length === 0 && !error ? (
        <div className="bg-white rounded-lg shadow p-8 text-center">
          <p className="text-gray-600 text-lg">No fabrics available at the moment.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Fabrics List */}
          <div className="lg:col-span-2">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {fabrics.map((fabric) => {
                const stockStatus = getStockStatus(fabric.stockQuantity)
                const quantity = quantities.get(fabric.fabricID) || 0
                const isInCart = cart.has(fabric.fabricID)

                return (
                  <div
                    key={fabric.fabricID}
                    className={`bg-white rounded-lg shadow p-6 hover:shadow-md transition-shadow ${
                      isInCart ? 'ring-2 ring-blue-500' : ''
                    }`}
                  >
                    <div className="flex items-start justify-between mb-4">
                      <h3 className="text-lg font-semibold text-gray-900">{fabric.fabricType}</h3>
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${stockStatus.color}`}
                      >
                        {stockStatus.text}
                      </span>
                    </div>

                    <div className="space-y-2 text-sm text-gray-600 mb-4">
                      {fabric.composition && (
                        <div>
                          <span className="font-medium text-gray-700">Composition:</span>{' '}
                          {fabric.composition}
                        </div>
                      )}
                      {fabric.color && (
                        <div>
                          <span className="font-medium text-gray-700">Color:</span> {fabric.color}
                        </div>
                      )}
                      {fabric.weightPerUnit && (
                        <div>
                          <span className="font-medium text-gray-700">Weight per Unit:</span>{' '}
                          {fabric.weightPerUnit} kg
                        </div>
                      )}
                      <div>
                        <span className="font-medium text-gray-700">Stock Quantity:</span>{' '}
                        <span className="font-semibold text-gray-900">{fabric.stockQuantity}</span>{' '}
                        rolls
                      </div>
                      {fabric.description && (
                        <div className="pt-2 border-t border-gray-200">
                          <p className="text-gray-600 text-xs">{fabric.description}</p>
                        </div>
                      )}
                    </div>

                    {/* Quantity Input */}
                    <div className="flex items-center gap-3 pt-4 border-t border-gray-200">
                      <label
                        htmlFor={`quantity-${fabric.fabricID}`}
                        className="text-sm font-medium text-gray-700"
                      >
                        Quantity:
                      </label>
                      <input
                        id={`quantity-${fabric.fabricID}`}
                        type="number"
                        min="0"
                        max={fabric.stockQuantity}
                        value={quantity}
                        onChange={(e) =>
                          handleQuantityChange(fabric.fabricID, parseInt(e.target.value) || 0)
                        }
                        disabled={fabric.stockQuantity === 0}
                        className="w-20 px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
                      />
                      <span className="text-sm text-gray-500">rolls</span>
                      {isInCart && (
                        <span className="ml-auto text-xs text-blue-600 font-medium">In Cart</span>
                      )}
                    </div>
                  </div>
                )
              })}
            </div>
          </div>

          {/* Cart Summary */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-lg shadow p-6 sticky top-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Order Summary</h3>

              {cartItems.length === 0 ? (
                <p className="text-gray-500 text-sm">No items in cart</p>
              ) : (
                <>
                  <div className="space-y-3 mb-4">
                    {cartItems.map((item) => (
                      <div
                        key={item.fabricID}
                        className="flex justify-between items-center text-sm border-b border-gray-200 pb-2"
                      >
                        <div className="flex-1">
                          <p className="font-medium text-gray-900">{item.fabricType}</p>
                          <p className="text-gray-500">Quantity: {item.quantity} rolls</p>
                        </div>
                        <button
                          onClick={() => handleQuantityChange(item.fabricID, 0)}
                          className="text-red-600 hover:text-red-800 text-xs font-medium"
                        >
                          Remove
                        </button>
                      </div>
                    ))}
                  </div>

                  <div className="pt-4 border-t border-gray-200">
                    <div className="flex justify-between items-center mb-4 text-sm">
                      <span className="font-medium text-gray-700">Total Items:</span>
                      <span className="font-semibold text-gray-900">
                        {cartItems.reduce((sum, item) => sum + item.quantity, 0)} rolls
                      </span>
                    </div>
                    <button
                      onClick={handleCreateOrder}
                      disabled={isSubmitting || cartItems.length === 0}
                      className="w-full bg-blue-600 text-white py-2 px-4 rounded-md font-medium hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
                    >
                      {isSubmitting ? 'Creating Order...' : 'Create Order'}
                    </button>
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default PurchaseFabric

