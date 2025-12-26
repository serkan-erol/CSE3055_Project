import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import Landing from './pages/Landing'
import CustomerLogin from './pages/CustomerLogin'
import CustomerRegister from './pages/CustomerRegister'
import EmployeeLogin from './pages/EmployeeLogin'
import CustomerDashboard from './pages/CustomerDashboard'
import EmployeeDashboard from './pages/EmployeeDashboard'
import PurchaseFabric from './pages/customer/PurchaseFabric'
import SupplyFabric from './pages/customer/SupplyFabric'
import CurrentOrders from './pages/customer/CurrentOrders'
import Settings from './pages/customer/Settings'
import PaymentMethods from './pages/customer/PaymentMethods'
import BankInformation from './pages/customer/BankInformation'
import MakePayment from './pages/customer/MakePayment'
import ViewSupplyPayments from './pages/customer/ViewSupplyPayments'
import CheckOrder from './pages/employee/CheckOrder'
import CheckShipment from './pages/employee/CheckShipment'
import ManageFabric from './pages/employee/ManageFabric'
import ManageFinance from './pages/employee/ManageFinance'
import ManagePayment from './pages/employee/ManagePayment'
import ManageTreasury from './pages/employee/ManageTreasury'
import ManageEmployee from './pages/employee/ManageEmployee'
import Views from './pages/employee/Views'

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Landing />} />
        <Route path="/customer/login" element={<CustomerLogin />} />
        <Route path="/customer/register" element={<CustomerRegister />} />
        <Route path="/employee/login" element={<EmployeeLogin />} />
        <Route path="/customer/dashboard" element={<CustomerDashboard />}>
          <Route index element={<div><h2 className="text-2xl font-bold text-gray-900 mb-4">Welcome to Customer Dashboard</h2><p className="text-gray-600">Select an option from the sidebar to get started.</p></div>} />
          <Route path="purchase-fabric" element={<PurchaseFabric />} />
          <Route path="supply-fabric" element={<SupplyFabric />} />
          <Route path="orders" element={<CurrentOrders />} />
          <Route path="make-payment" element={<MakePayment />} />
          <Route path="supply-payments" element={<ViewSupplyPayments />} />
          <Route path="payment-methods" element={<PaymentMethods />} />
          <Route path="bank-information" element={<BankInformation />} />
          <Route path="settings" element={<Settings />} />
        </Route>
        <Route path="/employee/dashboard" element={<EmployeeDashboard />}>
          <Route index element={<div><h2 className="text-2xl font-bold text-gray-900 mb-4">Welcome to Employee Dashboard</h2><p className="text-gray-600">Select an option from the sidebar to get started.</p></div>} />
          <Route path="check-order" element={<CheckOrder />} />
          <Route path="check-shipment" element={<CheckShipment />} />
          <Route path="manage-fabric" element={<ManageFabric />} />
          <Route path="manage-finance" element={<ManageFinance />} />
          <Route path="manage-payment" element={<ManagePayment />} />
          <Route path="manage-treasury" element={<ManageTreasury />} />
          <Route path="manage-employee" element={<ManageEmployee />} />
          <Route path="views" element={<Views />} />
          <Route path="settings" element={<Settings />} />
        </Route>
      </Routes>
    </Router>
  )
}

export default App
