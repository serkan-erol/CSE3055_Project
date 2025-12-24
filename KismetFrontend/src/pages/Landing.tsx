import { useNavigate } from 'react-router-dom'
import kismetLandingImage from '../assets/images/kismet_landing.jpg'

const Landing = () => {
  const navigate = useNavigate()

  return (
    <div className="min-h-screen bg-gradient-to-br from-primary-50 via-white to-primary-100">
      {/* Header with buttons */}
      <header className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <button
              onClick={() => navigate('/')}
              className="text-2xl font-bold text-primary-600 hover:text-primary-700 transition-colors cursor-pointer"
            >
              Kısmet
            </button>
            <div className="flex items-center space-x-4">
            <button
              onClick={() => navigate('/customer/register')}
              className="px-4 py-2 text-sm font-medium text-primary-700 bg-primary-50 border border-primary-300 rounded-md hover:bg-primary-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors"
            >
              Register
            </button>
            <button
              onClick={() => navigate('/customer/login')}
              className="px-4 py-2 text-sm font-medium text-white bg-primary-600 rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors"
            >
              Login
            </button>
            <button
              onClick={() => navigate('/employee/login')}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 border border-gray-300 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 transition-colors"
            >
              Employee Login
            </button>
            </div>
          </div>
        </div>
      </header>

      {/* Main content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="text-center">
          <h1 className="text-4xl font-extrabold text-gray-900 sm:text-5xl md:text-6xl">
            Welcome to{' '}
            <span className="text-primary-600">Kısmet</span>
          </h1>
          <p className="mt-3 max-w-md mx-auto text-base text-gray-500 sm:text-lg md:mt-5 md:text-xl md:max-w-3xl">
            Address for the best quality fabric
          </p>
          
          {/* Landing Image */}
          <div className="mt-8 md:mt-12 flex justify-center">
            <div className="relative max-w-4xl w-full">
              <img
                src={kismetLandingImage}
                alt="Kismet Fabric Collection"
                className="w-full h-auto rounded-lg shadow-2xl object-cover"
              />
            </div>
          </div>
        </div>
      </main>
    </div>
  )
}

export default Landing

