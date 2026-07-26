import { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import api from '../../api';
import Navbar from '../../components/Navbar';

const CategoriesManagement = () => {
  const [categories, setCategories] = useState([]);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  const fetchCategories = async () => {
    try {
      const response = await api.get('/admin/categories');
      setCategories(response.data);
    } catch (error) {
      console.error("Greška pri učitavanju kategorija:", error);
      toast.error("Nismo uspeli da učitamo kategorije.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchCategories();
  }, []);

  const handleCreateCategory = async (e) => {
    e.preventDefault();
    if (!name.trim()) {
      toast.error("Naziv kategorije je obavezan.");
      return;
    }

    try {
      setIsSubmitting(true);
      await api.post('/admin/categories', {
        name: name.trim(),
        description: description.trim()
      });

      toast.success("Kategorija je uspešno kreirana!");
      setName('');
      setDescription('');
      fetchCategories();
    } catch (error) {
      console.error("Greška pri kreiranju kategorije:", error);
      const errorMessage = error.response?.data || "Nismo uspeli da kreiramo kategoriju.";
      toast.error(typeof errorMessage === 'string' ? errorMessage : "Došlo je do greške.");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) return <div className="text-center mt-10">Učitavanje...</div>;

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="container mx-auto p-6 max-w-4xl">
        <h1 className="text-3xl font-bold text-gray-800 mb-6">Upravljanje kategorijama</h1>

        <form onSubmit={handleCreateCategory} className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 mb-8 flex flex-col gap-4">
          <h2 className="text-xl font-semibold text-gray-700">Dodaj novu kategoriju</h2>
          <div>
            <label className="block text-sm font-medium text-gray-600 mb-1">Naziv kategorije</label>
            <input 
              type="text" 
              value={name} 
              onChange={(e) => setName(e.target.value)} 
              required
              className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Npr. Koncerti"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-600 mb-1">Opis</label>
            <textarea 
              value={description} 
              onChange={(e) => setDescription(e.target.value)} 
              className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Kratak opis kategorije..."
            />
          </div>
          <button 
            type="submit" 
            disabled={isSubmitting}
            className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition self-end disabled:bg-blue-300"
          >
            {isSubmitting ? 'Čuvanje...' : 'Sačuvaj kategoriju'}
          </button>
        </form>

        <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-gray-100 border-b border-gray-200">
                <th className="p-4 font-semibold text-gray-600">Naziv</th>
                <th className="p-4 font-semibold text-gray-600">Opis</th>
              </tr>
            </thead>
            <tbody>
              {categories.length > 0 ? (
                categories.map((cat) => (
                  <tr key={cat.id} className="border-b border-gray-100 hover:bg-gray-50">
                    <td className="p-4 font-medium text-gray-800">{cat.name}</td>
                    <td className="p-4 text-gray-600">{cat.description || 'Nema opisa'}</td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan="2" className="p-6 text-center text-gray-500">Nema unetih kategorija.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default CategoriesManagement;