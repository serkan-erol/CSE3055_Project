import { useState, useEffect } from 'react'
import { financialTransactionApiCustomer, sessionApi } from '../../services/api'

interface FinancialTransaction {
  fTransactionID: number
  customerID: number
  transactionType: string
  totalAmount: number
  totalPaid: number
  remainingBalance: number
  paymentStatus: number
  description: string | null
  transactionDate: string
  lastUpdatedAt: string | null
}

const ViewSupplyPayments = () => {
  const [customerId, setCustomerId] = useState<number | null>(null)
  const [transactions, setTransactions] = useState<FinancialTransaction[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    const loadData = async () => {
      setIsLoading(true)
      try {
        const userInfo = await sessionApi.getCurrentUser()
        if (userInfo.userId) {
          setCustomerId(userInfo.userId)
          sessionStorage.setItem('userId', userInfo.userId.toString())
          
          // Load financial transactions
          const fts = await financialTransactionApiCustomer.getAll(userInfo.userId)
          const supplyFTs = Array.isArray(fts) 
            ? fts.filter((ft: FinancialTransaction) => ft.transactionType === 'Supply')
            : []
          setTransactions(supplyFTs)
        }
      } catch (err: any) {
        console.error('Error loading data:', err)
        setError(err.response?.data?.error || err.message || 'Failed to load transactions')
      } finally {
        setIsLoading(false)
      }
    }
    loadData()
  }, [])

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('tr-TR', {
      style: 'currency',
      currency: 'TRY',
    }).format(amount)
  }

  const formatDate = (dateString: string | null) => {
    if (!dateString) return 'N/A'
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
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

  return (
    <div className="max-w-6xl mx-auto">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Supply Payments</h2>
      <p className="text-gray-600 mb-6">View your Supply transactions and payment information</p>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-md p-4 mb-6">
          <div className="text-sm font-medium text-red-800">Error: {error}</div>
        </div>
      )}

      {/* Supply Transactions List */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <h3 className="text-lg font-semibold text-gray-800 mb-4">Supply Transactions</h3>
        
        {isLoading ? (
          <div className="text-gray-500">Loading transactions...</div>
        ) : transactions.length === 0 ? (
          <div className="text-gray-500">No Supply transactions found</div>
        ) : (
          <div className="space-y-4">
            {transactions.map((transaction) => {
              const status = getPaymentStatusDisplay(transaction.paymentStatus)
              
              return (
                <div
                  key={transaction.fTransactionID}
                  className="p-4 border-2 rounded-lg border-gray-200 bg-white"
                >
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-3">
                        <h4 className="font-semibold text-gray-800">
                          Transaction #{transaction.fTransactionID}
                        </h4>
                        <span className={`px-2 py-1 rounded text-xs font-medium ${status.color}`}>
                          {status.text}
                        </span>
                      </div>
                      <div className="grid grid-cols-2 gap-4 text-sm text-gray-600 mb-3">
                        <div>
                          <span className="font-medium">Total Amount:</span> {formatCurrency(transaction.totalAmount)}
                        </div>
                        <div>
                          <span className="font-medium">Amount Paid to You:</span> <span className="text-green-600 font-semibold">{formatCurrency(transaction.totalPaid)}</span>
                        </div>
                        <div>
                          <span className="font-medium">Remaining:</span> {formatCurrency(transaction.remainingBalance)}
                        </div>
                        <div>
                          <span className="font-medium">Date:</span> {formatDate(transaction.transactionDate)}
                        </div>
                      </div>
                      {transaction.description && (
                        <div className="mt-2 text-sm text-gray-500">
                          <span className="font-medium">Description:</span> {transaction.description}
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>

      {/* Empty State */}
      {transactions.length === 0 && !isLoading && (
        <div className="bg-white rounded-lg shadow-md p-12 text-center">
          <p className="text-gray-500 text-lg">No Supply transactions available</p>
        </div>
      )}
    </div>
  )
}

export default ViewSupplyPayments

