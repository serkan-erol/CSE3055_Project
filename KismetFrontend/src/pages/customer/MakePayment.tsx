import { useState, useEffect } from 'react'
import { paymentApi, paymentMethodApi, sessionApi, financialTransactionApiCustomer } from '../../services/api'

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

interface PaymentMethod {
  spmID?: number
  SPMID?: number
  cardNumber?: string
  CardNumber?: string
  cardType?: string
  CardType?: string
  cardExpirationDate?: string
  CardExpirationDate?: string
}

const MakePayment = () => {
  const [customerId, setCustomerId] = useState<number | null>(null)
  const [transactions, setTransactions] = useState<FinancialTransaction[]>([])
  const [paymentMethods, setPaymentMethods] = useState<PaymentMethod[]>([])
  const [selectedTransaction, setSelectedTransaction] = useState<FinancialTransaction | null>(null)
  const [paymentMethod, setPaymentMethod] = useState<'cash' | 'card'>('cash')
  const [selectedCardId, setSelectedCardId] = useState<number | null>(null)
  const [paymentAmount, setPaymentAmount] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  useEffect(() => {
    const loadData = async () => {
      try {
        const userInfo = await sessionApi.getCurrentUser()
        if (userInfo.userId) {
          setCustomerId(userInfo.userId)
          sessionStorage.setItem('userId', userInfo.userId.toString())
          
          // Load financial transactions
          const fts = await financialTransactionApiCustomer.getAll(userInfo.userId)
          const purchaseFTs = Array.isArray(fts) 
            ? fts.filter((ft: FinancialTransaction) => ft.transactionType === 'Purchase')
            : []
          setTransactions(purchaseFTs)
          
          // Load payment methods
          const methods = await paymentMethodApi.getAllByCustomerId(userInfo.userId)
          setPaymentMethods(Array.isArray(methods) ? methods : [])
        }
      } catch (err: any) {
        console.error('Error loading data:', err)
        setError(err.response?.data?.error || err.message || 'Failed to load data')
      }
    }
    loadData()
  }, [])

  const handleSelectTransaction = (transaction: FinancialTransaction) => {
    setSelectedTransaction(transaction)
    setPaymentAmount(transaction.remainingBalance.toString())
    setError('')
    setSuccess('')
  }

  const handleSelectCard = (card: PaymentMethod, index: number) => {
    // Use card ID if available, otherwise use index as fallback
    const cardId = card.spmID || card.SPMID || index
    
    console.log('Selecting card:', { card, cardId, index })
    setSelectedCardId(cardId)
    setError('')
  }

  const handleSubmitPayment = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!customerId || !selectedTransaction) {
      setError('Please select a transaction to pay')
      return
    }

    const amount = parseFloat(paymentAmount)
    if (isNaN(amount) || amount <= 0) {
      setError('Please enter a valid payment amount')
      return
    }

    if (amount > selectedTransaction.remainingBalance) {
      setError('Payment amount cannot exceed remaining balance')
      return
    }

    if (paymentMethod === 'card') {
      // If no card is selected, just select the first one automatically
      if (!selectedCardId && paymentMethods.length > 0) {
        const firstCard = paymentMethods[0]
        const firstCardId = firstCard.spmID || firstCard.SPMID || 0
        setSelectedCardId(firstCardId)
        console.log('Auto-selecting first card:', firstCardId)
      } else if (paymentMethods.length === 0) {
        setError('No saved cards available. Please add a payment method first.')
        setIsSubmitting(false)
        return
      }
    }

    setIsSubmitting(true)
    setError('')
    setSuccess('')

    try {
      const paymentData: any = {
        paymentAmount: amount,
        paymentType: selectedTransaction.transactionType
      }

      // Set payment method based on selection
      if (paymentMethod === 'cash') {
        paymentData.paymentMethod = 'cash'
      } else {
        // Card payment - just set to "card" or "Card"
        paymentData.paymentMethod = 'Card'
      }

      console.log('Creating payment with data:', {
        customerId,
        fTransactionId: selectedTransaction.fTransactionID,
        paymentData
      })
      
      const result = await paymentApi.create(customerId, selectedTransaction.fTransactionID, paymentData)
      console.log('Payment created successfully:', result)
      
      setSuccess('Payment created successfully!')
      setTimeout(() => {
        setSuccess('')
        setSelectedTransaction(null)
        setPaymentAmount('')
        setSelectedCardId(null)
        // Reload transactions to get updated balances
        if (customerId) {
          financialTransactionApiCustomer.getAll(customerId).then(fts => {
            const purchaseFTs = Array.isArray(fts) 
              ? fts.filter((ft: FinancialTransaction) => ft.transactionType === 'Purchase')
              : []
            setTransactions(purchaseFTs)
          })
        }
      }, 3000)
    } catch (err: any) {
      console.error('Error creating payment:', err)
      const errorMessage = err.response?.data?.error || err.message || 'Failed to create payment'
      setError(errorMessage)
    } finally {
      setIsSubmitting(false)
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

  const maskCardNumber = (cardNumber: string) => {
    if (!cardNumber) return ''
    const last4 = cardNumber.slice(-4)
    return `**** **** **** ${last4}`
  }

  return (
    <div className="max-w-4xl mx-auto">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Make Payment</h2>

      {/* Success Message */}
      {success && (
        <div className="mb-4 bg-green-50 border border-green-200 rounded-md p-4">
          <div className="text-sm font-medium text-green-800">{success}</div>
        </div>
      )}

      {/* Error Message */}
      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-4">
          <div className="text-sm font-medium text-red-800">Error: {error}</div>
        </div>
      )}

      {/* Financial Transactions List */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <h3 className="text-lg font-semibold text-gray-800 mb-4">Select a Purchase Transaction</h3>
        
        {isLoading ? (
          <div className="text-gray-500">Loading transactions...</div>
        ) : transactions.length === 0 ? (
          <div className="text-gray-500">No Purchase transactions found</div>
        ) : (
          <div className="space-y-3">
            {transactions.map((transaction) => {
              const status = getPaymentStatusDisplay(transaction.paymentStatus)
              const isSelected = selectedTransaction?.fTransactionID === transaction.fTransactionID
              
              return (
                <div
                  key={transaction.fTransactionID}
                  onClick={() => handleSelectTransaction(transaction)}
                  className={`p-4 border-2 rounded-lg cursor-pointer transition-colors ${
                    isSelected
                      ? 'border-primary-600 bg-primary-50'
                      : 'border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-2">
                        <h4 className="font-semibold text-gray-800">
                          Transaction #{transaction.fTransactionID}
                        </h4>
                        <span className={`px-2 py-1 rounded text-xs font-medium ${status.color}`}>
                          {status.text}
                        </span>
                      </div>
                      <div className="grid grid-cols-2 gap-4 text-sm text-gray-600">
                        <div>
                          <span className="font-medium">Total Amount:</span> {formatCurrency(transaction.totalAmount)}
                        </div>
                        <div>
                          <span className="font-medium">Total Paid:</span> {formatCurrency(transaction.totalPaid)}
                        </div>
                        <div>
                          <span className="font-medium">Remaining:</span> {formatCurrency(transaction.remainingBalance)}
                        </div>
                        <div>
                          <span className="font-medium">Date:</span> {formatDate(transaction.transactionDate)}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>

      {/* Payment Form */}
      {selectedTransaction && (
        <div className="bg-white rounded-lg shadow-md p-6">
          <h3 className="text-lg font-semibold text-gray-800 mb-4">Payment Details</h3>
          
          <form onSubmit={handleSubmitPayment} className="space-y-6">
            {/* Payment Method Selection */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Payment Method
              </label>
              <div className="flex gap-4">
                <label className="flex items-center">
                  <input
                    type="radio"
                    name="paymentMethod"
                    value="cash"
                    checked={paymentMethod === 'cash'}
                    onChange={(e) => {
                      setPaymentMethod('cash')
                      setSelectedCardId(null)
                      setError('')
                    }}
                    className="mr-2"
                  />
                  <span className="text-sm text-gray-700">Cash</span>
                </label>
                <label className="flex items-center">
                  <input
                    type="radio"
                    name="paymentMethod"
                    value="card"
                    checked={paymentMethod === 'card'}
                    onChange={(e) => {
                      setPaymentMethod('card')
                      // Don't reset selectedCardId here - user might have already selected a card
                      setError('')
                    }}
                    className="mr-2"
                  />
                  <span className="text-sm text-gray-700">Card</span>
                </label>
              </div>
            </div>

            {/* Payment Amount */}
            <div>
              <label htmlFor="paymentAmount" className="block text-sm font-medium text-gray-700 mb-2">
                Payment Amount <span className="text-red-500">*</span>
              </label>
              <input
                id="paymentAmount"
                type="number"
                min="0.01"
                max={selectedTransaction.remainingBalance}
                step="0.01"
                value={paymentAmount}
                onChange={(e) => setPaymentAmount(e.target.value)}
                required
                className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
                placeholder={`Max: ${formatCurrency(selectedTransaction.remainingBalance)}`}
              />
              <p className="mt-1 text-sm text-gray-500">
                Remaining Balance: {formatCurrency(selectedTransaction.remainingBalance)}
              </p>
            </div>

            {/* Card Payment Options */}
            {paymentMethod === 'card' && (
              <div className="space-y-4 border-t pt-4">
                <h4 className="font-medium text-gray-700">Select a Card</h4>
                
                {paymentMethods.length === 0 ? (
                  <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
                    <p className="text-sm text-yellow-800">
                      No saved cards found. Please add a payment method in the Payment Methods section.
                    </p>
                  </div>
                ) : (
                    <div className="space-y-2">
                      {paymentMethods.map((card, index) => {
                        const cardId = card.spmID || card.SPMID || index
                        const cardNumber = card.cardNumber || card.CardNumber || ''
                        const cardType = card.cardType || card.CardType || ''
                        const isSelected = selectedCardId === cardId
                        
                        return (
                          <div
                            key={cardId || `card-${index}`}
                            onClick={() => {
                              console.log('Card clicked:', { card, index, cardId })
                              handleSelectCard(card, index)
                            }}
                            className={`p-4 border-2 rounded-lg cursor-pointer transition-colors ${
                              isSelected
                                ? 'border-primary-600 bg-primary-50'
                                : 'border-gray-200 hover:border-gray-300'
                            }`}
                          >
                            <div className="flex items-center justify-between">
                              <div>
                                <span className="font-medium text-gray-800">{cardType}</span>
                                <span className="ml-2 text-gray-600">{maskCardNumber(cardNumber)}</span>
                              </div>
                              {isSelected && (
                                <span className="text-primary-600 text-sm font-semibold">✓ Selected</span>
                              )}
                            </div>
                          </div>
                        )
                      })}
                    </div>
                )}
              </div>
            )}

            {/* Submit Button */}
            <div className="flex justify-end">
              <button
                type="submit"
                disabled={isSubmitting}
                className="px-6 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isSubmitting ? 'Processing...' : 'Pay'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Empty State */}
      {!selectedTransaction && transactions.length === 0 && !isLoading && (
        <div className="bg-white rounded-lg shadow-md p-12 text-center">
          <p className="text-gray-500 text-lg">No Purchase transactions available for payment</p>
        </div>
      )}
    </div>
  )
}

export default MakePayment

