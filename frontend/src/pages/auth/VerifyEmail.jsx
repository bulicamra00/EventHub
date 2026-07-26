import { useEffect, useState, useRef } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import api from '../../api'; 

const VerifyEmail = () => {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const [status, setStatus] = useState('loading');
  const hasCalled = useRef(false); 

  useEffect(() => {
    if (hasCalled.current) return; 
    hasCalled.current = true;

    const verify = async () => {
      if (!token) {
        setStatus('error');
        return;
      }

      try {
        await api.get(`/Users/confirm-email?token=${token}`);
        setStatus('success');
      } catch (error) {
        setStatus('error');
      }
    };

    verify();
  }, [token]);

  return (
    <div className="flex justify-center items-center min-h-screen bg-gray-100">
      <div className="bg-white p-8 rounded-lg shadow-md w-96 text-center">
        {status === 'loading' && <h2 className="text-xl">Potvrđujem nalog...</h2>}
        
        {status === 'success' && (
          <div>
            <h2 className="text-2xl font-bold text-green-600">Uspešno!</h2>
            <p className="mt-2 text-gray-600">Email je potvrđen. Možete se ulogovati.</p>
            <Link to="/login" className="mt-6 block bg-blue-600 text-white py-2 rounded font-bold hover:bg-blue-700">
              Idi na Prijavu
            </Link>
          </div>
        )}

        {status === 'error' && (
          <div>
            <h2 className="text-2xl font-bold text-red-600">Greška</h2>
            <p className="mt-2 text-gray-600">Potvrda nije uspela ili je token nevažeći.</p>
            <Link to="/register" className="mt-6 block bg-blue-600 text-white py-2 rounded font-bold hover:bg-blue-700">
              Nazad na registraciju
            </Link>
          </div>
        )}
      </div>
    </div>
  );
};

export default VerifyEmail;