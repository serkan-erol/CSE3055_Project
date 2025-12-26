import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { fabricApi, sessionApi } from '../../services/api'

interface Fabric {
  fabricID: number
  fabricType: string
  composition: string | null
  color: string | null
  weightPerUnit: number | null
  stockQuantity: number
  description: string | null
}

const ManageFabric = () => {
  const navigate = useNavigate()
  const [employeeId, setEmployeeId] = useState<number | null>(null)
  const [accessLevel, setAccessLevel] = useState<number | null>(null)
  const [fabrics, setFabrics] = useState<Fabric[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  
  // Create fabric form state
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [isCreating, setIsCreating] = useState(false)
  const [createError, setCreateError] = useState('')
  const [createSuccess, setCreateSuccess] = useState('')
  const [createForm, setCreateForm] = useState({
    fabricType: '',
    composition: '',
    color: '',
    weightPerUnit: '',
    stockQuantity: '',
    unitPrice: '',
    description: ''
  })

  // Update stock state
  const [updatingFabricId, setUpdatingFabricId] = useState<number | null>(null)
  const [stockChange, setStockChange] = useState<{ [key: number]: string }>({})
  const [isUpdatingStock, setIsUpdatingStock] = useState(false)

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
          
          // Check access level - require at least 5
          if (level < 5) {
            navigate('/employee/dashboard', {
              state: { message: 'You do not have permission to access this page. Access Level 5 or higher is required.' }
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
            if (userInfo.accessLevel < 5) {
              navigate('/employee/dashboard', {
                state: { message: 'You do not have permission to access this page. Access Level 5 or higher is required.' }
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

  // Load fabrics on mount
  useEffect(() => {
    if (accessLevel !== null && accessLevel >= 5) {
      loadFabrics()
    }
  }, [accessLevel])

  const loadFabrics = async () => {
    setIsLoading(true)
    setError('')
    try {
      const fabricsData = await fabricApi.getAll()
      setFabrics(Array.isArray(fabricsData) ? fabricsData : [])
    } catch (err: any) {
      console.error('Error loading fabrics:', err)
      const errorMessage = err.response?.data?.error || err.message || 'Failed to load fabrics'
      setError(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }

  const handleCreateFabric = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!employeeId) {
      setCreateError('Unable to create fabric. Please refresh the page.')
      return
    }

    setIsCreating(true)
    setCreateError('')
    setCreateSuccess('')

    try {
      const fabricData = {
        fabricType: createForm.fabricType.trim(),
        composition: createForm.composition.trim() || undefined,
        color: createForm.color.trim() || undefined,
        weightPerUnit: createForm.weightPerUnit ? parseFloat(createForm.weightPerUnit) : undefined,
        stockQuantity: parseInt(createForm.stockQuantity),
        unitPrice: parseFloat(createForm.unitPrice),
        description: createForm.description.trim() || undefined
      }

      // Validate required fields
      if (!fabricData.fabricType) {
        setCreateError('Fabric Type is required')
        setIsCreating(false)
        return
      }
      if (isNaN(fabricData.stockQuantity) || fabricData.stockQuantity < 0) {
        setCreateError('Stock Quantity must be a valid non-negative number')
        setIsCreating(false)
        return
      }
      if (isNaN(fabricData.unitPrice) || fabricData.unitPrice < 0) {
        setCreateError('Unit Price must be a valid non-negative number')
        setIsCreating(false)
        return
      }

      await fabricApi.create(employeeId, fabricData)
      
      // Reset form
      setCreateForm({
        fabricType: '',
        composition: '',
        color: '',
        weightPerUnit: '',
        stockQuantity: '',
        unitPrice: '',
        description: ''
      })
      setShowCreateForm(false)
      setCreateSuccess('Fabric created successfully!')
      setTimeout(() => setCreateSuccess(''), 5000)
      
      // Reload fabrics
      await loadFabrics()
    } catch (err: any) {
      console.error('Error creating fabric:', err)
      const errorMessage = err.response?.data?.error || err.message || 'Failed to create fabric'
      setCreateError(errorMessage)
    } finally {
      setIsCreating(false)
    }
  }

  const handleUpdateStock = async (fabricId: number) => {
    if (!employeeId) {
      setError('Unable to update stock. Please refresh the page.')
      return
    }

    const changeValue = stockChange[fabricId]
    if (!changeValue || changeValue.trim() === '') {
      setError('Please enter a quantity change')
      return
    }

    const quantityChange = parseInt(changeValue)
    if (isNaN(quantityChange) || quantityChange === 0) {
      setError('Quantity change must be a non-zero number')
      return
    }

    setIsUpdatingStock(true)
    setUpdatingFabricId(fabricId)
    setError('')

    try {
      await fabricApi.updateStock(fabricId, quantityChange)
      
      // Clear the input
      setStockChange({ ...stockChange, [fabricId]: '' })
      
      // Reload fabrics
      await loadFabrics()
      
      setCreateSuccess(`Stock updated successfully! ${quantityChange > 0 ? 'Added' : 'Removed'} ${Math.abs(quantityChange)} units.`)
      setTimeout(() => setCreateSuccess(''), 5000)
    } catch (err: any) {
      console.error('Error updating stock:', err)
      const errorMessage = err.response?.data?.error || err.response?.data?.message || err.message || 'Failed to update stock'
      setError(errorMessage)
    } finally {
      setIsUpdatingStock(false)
      setUpdatingFabricId(null)
    }
  }

  if (accessLevel === null || accessLevel < 5) {
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
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Manage Fabric</h2>
        <button
          onClick={() => {
            setShowCreateForm(!showCreateForm)
            setCreateError('')
            setCreateSuccess('')
          }}
          className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors"
        >
          {showCreateForm ? 'Cancel' : 'Create New Fabric'}
        </button>
      </div>

      {/* Success Message */}
      {createSuccess && (
        <div className="mb-4 bg-green-50 border border-green-200 rounded-md p-4">
          <div className="text-sm font-medium text-green-800">{createSuccess}</div>
        </div>
      )}

      {/* Error Message */}
      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-4">
          <div className="text-sm font-medium text-red-800">Error: {error}</div>
        </div>
      )}

      {/* Create Fabric Form */}
      {showCreateForm && (
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <h3 className="text-lg font-semibold text-gray-800 mb-4">Create New Fabric</h3>
          
          {createError && (
            <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-4">
              <div className="text-sm font-medium text-red-800">Error: {createError}</div>
            </div>
          )}

          <form onSubmit={handleCreateFabric} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label htmlFor="fabricType" className="block text-sm font-medium text-gray-700 mb-1">
                  Fabric Type <span className="text-red-500">*</span>
                </label>
                <input
                  id="fabricType"
                  type="text"
                  value={createForm.fabricType}
                  onChange={(e) => setCreateForm({ ...createForm, fabricType: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>

              <div>
                <label htmlFor="stockQuantity" className="block text-sm font-medium text-gray-700 mb-1">
                  Stock Quantity <span className="text-red-500">*</span>
                </label>
                <input
                  id="stockQuantity"
                  type="number"
                  min="0"
                  value={createForm.stockQuantity}
                  onChange={(e) => setCreateForm({ ...createForm, stockQuantity: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>

              <div>
                <label htmlFor="unitPrice" className="block text-sm font-medium text-gray-700 mb-1">
                  Unit Price <span className="text-red-500">*</span>
                </label>
                <input
                  id="unitPrice"
                  type="number"
                  min="0"
                  step="0.01"
                  value={createForm.unitPrice}
                  onChange={(e) => setCreateForm({ ...createForm, unitPrice: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>

              <div>
                <label htmlFor="composition" className="block text-sm font-medium text-gray-700 mb-1">
                  Composition
                </label>
                <input
                  id="composition"
                  type="text"
                  value={createForm.composition}
                  onChange={(e) => setCreateForm({ ...createForm, composition: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>

              <div>
                <label htmlFor="color" className="block text-sm font-medium text-gray-700 mb-1">
                  Color
                </label>
                <input
                  id="color"
                  type="text"
                  value={createForm.color}
                  onChange={(e) => setCreateForm({ ...createForm, color: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>

              <div>
                <label htmlFor="weightPerUnit" className="block text-sm font-medium text-gray-700 mb-1">
                  Weight Per Unit
                </label>
                <input
                  id="weightPerUnit"
                  type="number"
                  min="0"
                  step="0.01"
                  value={createForm.weightPerUnit}
                  onChange={(e) => setCreateForm({ ...createForm, weightPerUnit: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>
            </div>

            <div>
              <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
                Description
              </label>
              <textarea
                id="description"
                value={createForm.description}
                onChange={(e) => setCreateForm({ ...createForm, description: e.target.value })}
                rows={3}
                className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
            </div>

            <div className="flex justify-end gap-4">
              <button
                type="button"
                onClick={() => {
                  setShowCreateForm(false)
                  setCreateError('')
                }}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isCreating}
                className="px-4 py-2 text-sm font-medium text-white bg-primary-600 rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isCreating ? 'Creating...' : 'Create Fabric'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Fabrics List */}
      <div className="bg-white rounded-lg shadow-md overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-800">Fabric Inventory</h3>
        </div>

        {isLoading ? (
          <div className="p-8 text-center">
            <div className="text-gray-500">Loading fabrics...</div>
          </div>
        ) : fabrics.length === 0 ? (
          <div className="p-8 text-center">
            <div className="text-gray-500">No fabrics found</div>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ID</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Composition</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Color</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Stock</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Update Stock</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {fabrics.map((fabric) => (
                  <tr key={fabric.fabricID}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{fabric.fabricID}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{fabric.fabricType}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{fabric.composition || 'N/A'}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{fabric.color || 'N/A'}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{fabric.stockQuantity}</td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center gap-2">
                        <input
                          type="number"
                          placeholder="±Change"
                          value={stockChange[fabric.fabricID] || ''}
                          onChange={(e) => setStockChange({ ...stockChange, [fabric.fabricID]: e.target.value })}
                          className="w-24 px-2 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                          disabled={isUpdatingStock}
                        />
                        <button
                          onClick={() => handleUpdateStock(fabric.fabricID)}
                          disabled={isUpdatingStock && updatingFabricId === fabric.fabricID}
                          className="px-3 py-1 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                          {isUpdatingStock && updatingFabricId === fabric.fabricID ? 'Updating...' : 'Update'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}

export default ManageFabric

