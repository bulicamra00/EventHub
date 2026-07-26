import { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import api from "../../api";
import toast from 'react-hot-toast';

const Login = () => {
  const [formData, setFormData] = useState({ email: '', password: '' });
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);

    try {
      const response = await api.post('/Users/login', formData);
      const token = response.data.accessToken; 
      
      if (token) {
        localStorage.setItem('token', token);
        window.dispatchEvent(new Event('authChanged'));
        toast.success('Uspešna prijava!');

        const redirectTo = searchParams.get('redirect');
        navigate(redirectTo || '/');
        
      } else {
        toast.error('Došlo je do greške: Token nije primljen.');
      }
    } catch (error) {
      console.error('Greška pri prijavi:', error);
      toast.error('Pogrešan email ili lozinka.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex justify-center items-center min-h-screen bg-gray-100">
      <div className="bg-white p-8 rounded-lg shadow-md w-96">
        <h2 className="text-2xl font-bold mb-6 text-center text-blue-600">Prijava</h2>
        
        <form onSubmit={handleSubmit} className="space-y-4">
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
              isLoading ? 'bg-gray-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700'
            }`}
          >
            {isLoading ? 'Prijava u toku...' : 'Uloguj se'}
          </button>
        </form>

        <p className="mt-4 text-center text-sm text-gray-600">
          Nemate nalog?{' '} 
          <Link to="/register" className="text-blue-600 font-bold hover:underline">
            Registrujte se
          </Link>
        </p>
      </div>
    </div>
  );
};

export default Login;