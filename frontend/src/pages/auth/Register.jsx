import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from '../../api'; 
import toast from 'react-hot-toast';

const Register = () => {
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    password: '',
    city: ''
  });
  
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true); 
    
    try {
      await api.post('/Users/register', formData);
      
      toast.success('Registracija je uspešna! Proverite svoj mejl i potvrdite nalog.', {
        duration: 6000
      });
      
      navigate('/login');
    } catch (error) {
      console.error('Greška pri registraciji:', error);
      toast.error('Došlo je do greške. Proverite da li su podaci ispravni.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex justify-center items-center min-h-screen bg-gray-100">
      <div className="bg-white p-8 rounded-lg shadow-md w-96">
        <h2 className="text-2xl font-bold mb-6 text-center text-blue-600">Registracija</h2>
        
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">Ime i prezime</label>
            <input 
              name="fullName" 
              type="text" 
              onChange={handleChange} 
              required 
              disabled={isLoading}
              className="w-full border border-gray-300 rounded p-2 mt-1" 
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700">Email</label>
            <input 
              name="email" 
              type="email" 
              onChange={handleChange} 
              required 
              disabled={isLoading}
              className="w-full border border-gray-300 rounded p-2 mt-1" 
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700">Grad</label>
            <input 
              name="city" 
              type="text" 
              onChange={handleChange} 
              required 
              disabled={isLoading}
              className="w-full border border-gray-300 rounded p-2 mt-1" 
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700">Lozinka</label>
            <input 
              name="password" 
              type="password" 
              onChange={handleChange} 
              required 
              disabled={isLoading}
              className="w-full border border-gray-300 rounded p-2 mt-1" 
            />
          </div>

          <button 
            type="submit" 
            disabled={isLoading}
            className={`w-full py-2 rounded font-bold text-white transition-colors ${
              isLoading ? 'bg-gray-400 cursor-not-allowed' : 'bg-green-600 hover:bg-green-700'
            }`}
          >
            {isLoading ? 'Registracija u toku...' : 'Registruj se'}
          </button>
        </form>

        <p className="mt-4 text-center text-sm text-gray-600">
          Već imate nalog?{' '}
          <Link to="/login" className="text-blue-600 font-bold hover:underline">
            Ulogujte se ovde
          </Link>
        </p>
      </div>
    </div>
  );
};

export default Register;