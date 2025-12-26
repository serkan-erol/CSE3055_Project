import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { financialTransactionApi, sessionApi } from '../../services/api'

interface FinancialTransaction {
  fTransactionID: number
  customerID: number
  billingID: number
  orderID: number
  transactionType: string
  totalAmount: number
  totalPaid: number
  remainingBalance: number
  paymentStatus: number
  description: string | null
  transactionDate: string
  lastUpdatedAt: string | null
}

const ManageFinance = () => {
  const navigate = useNavigate()
  const [employeeId, setEmployeeId] = useState<number | null>(null)
  const [accessLevel, setAccessLevel] = useState<number | null>(null)
  const [searchType, setSearchType] = useState<'customer' | 'transaction'>('transaction')
  const [customerId, setCustomerId] = useState('')
  const [fTransactionId, setFTransactionId] = useState('')
  const [transaction, setTransaction] = useState<FinancialTransaction | null>(null)
  const [transactions, setTransactions] = useState<FinancialTransaction[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const [isUpdating, setIsUpdating] = useState(false)
  const [updateError, setUpdateError] = useState('')
  const [updateSuccess, setUpdateSuccess] = useState('')
  const [editingDescription, setEditingDescription] = useState(false)
  const [newDescription, setNewDescription] = useState('')

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
    if (!employeeId) {
      setError('Unable to search. Please refresh the page.')
      return
    }

    setError('')
    setTransaction(null)
    setTransactions([])
    setIsLoading(true)

    try {
      if (searchType === 'transaction') {
        if (!fTransactionId.trim()) {
          setError('Please enter a Financial Transaction ID')
          setIsLoading(false)
          return
        }

        const parsedId = parseInt(fTransactionId)
        if (isNaN(parsedId) || parsedId <= 0) {
          setError('Please enter a valid Financial Transaction ID')
          setIsLoading(false)
          return
        }

        const data = await financialTransactionApi.getById(employeeId, parsedId)
        setTransaction(data)
        setNewDescription(data.description || '')
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

        const data = await financialTransactionApi.getByCustomerId(employeeId, parsedId)
        setTransactions(Array.isArray(data) ? data : [])
      }
    } catch (err: any) {
      console.error('Error searching:', err)
      const errorMessage = err.response?.data?.error || err.message || 'Failed to search'
      setError(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }

  const handleUpdateDescription = async () => {
    if (!employeeId || !transaction) {
      setUpdateError('Unable to update. Please refresh the page.')
      return
    }

    setIsUpdating(true)
    setUpdateError('')
    setUpdateSuccess('')

    try {
      await financialTransactionApi.updateDescription(employeeId, transaction.fTransactionID, newDescription)
      setTransaction({ ...transaction, description: newDescription })
      setEditingDescription(false)
      setUpdateSuccess('Description updated successfully!')
      setTimeout(() => setUpdateSuccess(''), 5000)
    } catch (err: any) {
      console.error('Error updating description:', err)
      const errorMessage = err.response?.data?.error || err.message || 'Failed to update description'
      setUpdateError(errorMessage)
    } finally {
      setIsUpdating(false)
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

  const getPaymentStatusDisplay = (status: number): { text: string; color: string } => {
    switch (status) {
      case 0:
        return { text: 'Unpaid', color: 'bg-red-100 text-red-800' }
      case 1:
        return { text: 'Partial', color: 'bg-yellow-100 text-yellow-800' }
      case 2:
        return { text: 'Paid', color: 'bg-green-100 text-green-800' }
      default:
        return { text: 'Unknown', color: 'bg-gray-100 text-gray-800' }
    }
  }

  const renderTransactionDetails = (ft: FinancialTransaction) => {
    const status = getPaymentStatusDisplay(ft.paymentStatus)
    const isEditing = editingDescription && transaction?.fTransactionID === ft.fTransactionID

    return (
      <div className="bg-white rounded-lg shadow-md p-6 mb-4">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-xl font-semibold text-gray-800">Financial Transaction #{ft.fTransactionID}</h3>
          <span className={`px-3 py-1 rounded-full text-sm font-medium ${status.color}`}>
            {status.text}
          </span>
        </div>

        {updateSuccess && (
          <div className="mb-4 bg-green-50 border border-green-200 rounded-md p-4">
            <div className="text-sm font-medium text-green-800">{updateSuccess}</div>
          </div>
        )}

        {updateError && (
          <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-4">
            <div className="text-sm font-medium text-red-800">Error: {updateError}</div>
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-4">
            <h4 className="text-lg font-semibold text-gray-700 border-b pb-2">Transaction Information</h4>
            <div className="space-y-2 text-gray-700">
              <p><strong>Transaction ID:</strong> {ft.fTransactionID}</p>
              <p><strong>Customer ID:</strong> {ft.customerID}</p>
              <p><strong>Billing ID:</strong> {ft.billingID}</p>
              <p><strong>Order ID:</strong> {ft.orderID}</p>
              <p><strong>Transaction Type:</strong> {ft.transactionType}</p>
              <p><strong>Transaction Date:</strong> {formatDate(ft.transactionDate)}</p>
            </div>
          </div>

          <div className="space-y-4">
            <h4 className="text-lg font-semibold text-gray-700 border-b pb-2">Financial Details</h4>
            <div className="space-y-2 text-gray-700">
              <p><strong>Total Amount:</strong> {formatCurrency(ft.totalAmount)}</p>
              <p><strong>Total Paid:</strong> {formatCurrency(ft.totalPaid)}</p>
              <p><strong>Remaining Balance:</strong> {formatCurrency(ft.remainingBalance)}</p>
              {ft.lastUpdatedAt && (
                <p><strong>Last Updated:</strong> {formatDate(ft.lastUpdatedAt)}</p>
              )}
            </div>
          </div>
        </div>

        <div className="mt-6">
          <h4 className="text-lg font-semibold text-gray-700 border-b pb-2 mb-4">Description</h4>
          {isEditing ? (
            <div className="space-y-2">
              <textarea
                value={newDescription}
                onChange={(e) => setNewDescription(e.target.value)}
                rows={3}
                className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
              <div className="flex gap-2">
                <button
                  onClick={handleUpdateDescription}
                  disabled={isUpdating}
                  className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isUpdating ? 'Updating...' : 'Save'}
                </button>
                <button
                  onClick={() => {
                    setEditingDescription(false)
                    setNewDescription(ft.description || '')
                    setUpdateError('')
                  }}
                  className="px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300"
                >
                  Cancel
                </button>
              </div>
            </div>
          ) : (
            <div className="flex justify-between items-start">
              <p className="text-gray-700 flex-1">{ft.description || 'No description'}</p>
              <button
                onClick={() => {
                  setEditingDescription(true)
                  setNewDescription(ft.description || '')
                  setUpdateError('')
                  setUpdateSuccess('')
                }}
                className="ml-4 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
              >
                Edit Description
              </button>
            </div>
          )}
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
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Manage Finance</h2>

      {/* Search Form */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <form onSubmit={handleSearch} className="space-y-4">
          <div className="flex gap-4 mb-4">
            <label className="flex items-center">
              <input
                type="radio"
                name="searchType"
                value="transaction"
                checked={searchType === 'transaction'}
                onChange={(e) => {
                  setSearchType('transaction')
                  setTransaction(null)
                  setTransactions([])
                  setError('')
                }}
                className="mr-2"
              />
              <span className="text-sm font-medium text-gray-700">Search by Transaction ID</span>
            </label>
            <label className="flex items-center">
              <input
                type="radio"
                name="searchType"
                value="customer"
                checked={searchType === 'customer'}
                onChange={(e) => {
                  setSearchType('customer')
                  setTransaction(null)
                  setTransactions([])
                  setError('')
                }}
                className="mr-2"
              />
              <span className="text-sm font-medium text-gray-700">Search by Customer ID</span>
            </label>
          </div>

          <div className="flex gap-4">
            {searchType === 'transaction' ? (
              <div className="flex-1">
                <label htmlFor="fTransactionId" className="block text-sm font-medium text-gray-700 mb-2">
                  Financial Transaction ID
                </label>
                <input
                  id="fTransactionId"
                  type="text"
                  value={fTransactionId}
                  onChange={(e) => setFTransactionId(e.target.value)}
                  placeholder="Enter transaction ID"
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

      {/* Single Transaction */}
      {transaction && renderTransactionDetails(transaction)}

      {/* Multiple Transactions */}
      {transactions.length > 0 && (
        <div className="space-y-4">
          <h3 className="text-lg font-semibold text-gray-800">
            Found {transactions.length} transaction(s) for Customer #{customerId}
          </h3>
          {transactions.map((ft) => (
            <div key={ft.fTransactionID}>
              {renderTransactionDetails(ft)}
            </div>
          ))}
        </div>
      )}

      {!transaction && transactions.length === 0 && !error && !isLoading && (
        <div className="bg-white rounded-lg shadow-md p-12 text-center">
          <p className="text-gray-500 text-lg">
            {searchType === 'transaction' 
              ? 'Enter a Financial Transaction ID above to view details'
              : 'Enter a Customer ID above to view transactions'}
          </p>
        </div>
      )}
    </div>
  )
}

export default ManageFinance

