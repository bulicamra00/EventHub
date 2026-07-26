import { useState, useEffect } from 'react';
import api from '../../api';

const EventForm = ({ initialValues, onSubmit, isSubmitting, onFileChange }) => {
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    startDate: '',
    endDate: '',
    location: '',
    latitude: '',
    longitude: '',
    onlineLink: '',
    coverImageUrl: '',
    categoryId: '',
    isPrivate: false,
    isRecurring: false,     
    numberOfWeeks: 4,       
    tagNames: [],
    ticketTypes: []
  });

  const [categories, setCategories] = useState([]);
  const [tagInput, setTagInput] = useState('');

  useEffect(() => {
    if (initialValues) {
      setFormData({
        title: initialValues.title || '',
        description: initialValues.description || '',
        startDate: initialValues.startDate ? initialValues.startDate.slice(0, 16) : '',
        endDate: initialValues.endDate ? initialValues.endDate.slice(0, 16) : '',
        location: initialValues.location || '',
        latitude: initialValues.latitude ?? '',
        longitude: initialValues.longitude ?? '',
        onlineLink: initialValues.onlineLink || '',
        coverImageUrl: initialValues.coverImageUrl || '',
        categoryId: initialValues.categoryId || initialValues.category?.id || '',
        isPrivate: initialValues.isPrivate || false,
        isRecurring: false, 
        numberOfWeeks: 4,
        tagNames: initialValues.tagNames || [],
        ticketTypes: (initialValues.ticketTypes || []).map(t => ({
          ...t,
          earlyBirdExpiryDate: t.earlyBirdExpiryDate ? t.earlyBirdExpiryDate.slice(0, 16) : ''
        }))
      });

      if (initialValues.tagNames && Array.isArray(initialValues.tagNames)) {
        setTagInput(initialValues.tagNames.join(', '));
      }
    }
  }, [initialValues]);

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const res = await api.get('/categories');
        setCategories(res.data);
      } catch (err) {
        console.error("Greška pri učitavanju kategorija:", err);
      }
    };
    fetchCategories();
  }, []);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    if (name === 'tagNames') {
      setTagInput(value);
      setFormData(prev => ({
        ...prev,
        tagNames: value.split(',').map(tag => tag.trim()).filter(tag => tag !== "")
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: type === 'checkbox' ? checked : value
      }));
    }
  };

  const handleFileChange = (e) => {
    if (e.target.files && e.target.files[0]) {
      onFileChange(e.target.files[0]);
    }
  };

  const addTicketType = () => {
    setFormData(prev => ({
      ...prev,
      ticketTypes: [...prev.ticketTypes, { name: '', price: 0, earlyBirdPrice: '', earlyBirdExpiryDate: '', capacity: 0 }]
    }));
  };

  const updateTicket = (index, field, value) => {
    const newTickets = [...formData.ticketTypes];
    if (field === 'name') {
      newTickets[index][field] = value;
    } else if (field === 'earlyBirdExpiryDate') {
      newTickets[index][field] = value;
    } else {
      newTickets[index][field] = value === '' ? '' : parseFloat(value) || 0;
    }
    setFormData(prev => ({ ...prev, ticketTypes: newTickets }));
  };

  const removeTicket = (index) => {
    setFormData(prev => ({
      ...prev,
      ticketTypes: prev.ticketTypes.filter((_, i) => i !== index)
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(formData);
  };

  const inputClass = "w-full mt-1 p-2.5 bg-gray-50 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 transition-all outline-none";
  const labelClass = "block text-sm font-semibold text-gray-700";

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      
      <div>
        <label className={labelClass}>Naziv događaja</label>
        <input name="title" value={formData.title} onChange={handleChange} className={inputClass} required placeholder="Unesite naziv..." />
      </div>

      <div>
        <label className={labelClass}>Opis</label>
        <textarea name="description" value={formData.description} onChange={handleChange} className={`${inputClass} h-24 resize-none`} placeholder="Opišite događaj..." />
      </div>

      <div>
        <label className={labelClass}>Tagovi (razdvojeni zarezom)</label>
        <input name="tagNames" value={tagInput} onChange={handleChange} className={inputClass} placeholder="npr. jahanje, priroda, avantura" />
      </div>

      <div className="space-y-3 p-4 bg-gray-50 rounded-lg border border-gray-200">
        <label className={labelClass}>Tipovi ulaznica</label>
        {formData.ticketTypes.map((t, i) => (
          <div key={i} className="flex flex-col gap-2 p-3 bg-white border border-gray-200 rounded-lg">
            <div className="flex gap-2 items-center">
              <input placeholder="Naziv ulaznice" value={t.name} onChange={(e) => updateTicket(i, 'name', e.target.value)} className="w-full p-2 border rounded text-sm" />
              <input type="number" placeholder="Cena" value={t.price} onChange={(e) => updateTicket(i, 'price', e.target.value)} className="w-24 p-2 border rounded text-sm" />
              <input type="number" placeholder="Kapacitet" value={t.capacity} onChange={(e) => updateTicket(i, 'capacity', e.target.value)} className="w-24 p-2 border rounded text-sm" />
              <button type="button" onClick={() => removeTicket(i)} className="text-red-500 font-bold px-2">X</button>
            </div>
            <div className="flex gap-2 items-center">
              <input type="number" placeholder="Early Bird Cena" value={t.earlyBirdPrice ?? ''} onChange={(e) => updateTicket(i, 'earlyBirdPrice', e.target.value)} className="w-1/2 p-2 border rounded text-sm" />
              <input type="datetime-local" placeholder="Važi do" value={t.earlyBirdExpiryDate ?? ''} onChange={(e) => updateTicket(i, 'earlyBirdExpiryDate', e.target.value)} className="w-1/2 p-2 border rounded text-sm" />
            </div>
          </div>
        ))}
        <button type="button" onClick={addTicketType} className="text-sm text-blue-600 font-semibold">+ Dodaj ulaznicu</button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <label className={labelClass}>Početak</label>
          <input type="datetime-local" name="startDate" value={formData.startDate} onChange={handleChange} className={inputClass} required />
        </div>
        <div>
          <label className={labelClass}>Kraj</label>
          <input type="datetime-local" name="endDate" value={formData.endDate} onChange={handleChange} className={inputClass} required />
        </div>
      </div>

      <div>
        <label className={labelClass}>Lokacija</label>
        <input name="location" value={formData.location} onChange={handleChange} className={inputClass} placeholder="Grad, adresa..." />
      </div>

      <div className="grid grid-cols-2 gap-6">
        <div>
          <label className={labelClass}>Latitude</label>
          <input name="latitude" type="number" step="any" value={formData.latitude} onChange={handleChange} className={inputClass} />
        </div>
        <div>
          <label className={labelClass}>Longitude</label>
          <input name="longitude" type="number" step="any" value={formData.longitude} onChange={handleChange} className={inputClass} />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <label className={labelClass}>Online link</label>
          <input name="onlineLink" value={formData.onlineLink} onChange={handleChange} className={inputClass} placeholder="https://..." />
        </div>
        <div>
          <label className={labelClass}>Naslovna slika</label>
          <input type="file" accept="image/*" onChange={handleFileChange} className="w-full mt-1 p-2 text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100 cursor-pointer" />
        </div>
      </div>

      <div>
        <label className={labelClass}>Kategorija</label>
        <select name="categoryId" value={formData.categoryId} onChange={handleChange} className={inputClass} required>
          <option value="">Izaberi kategoriju</option>
          {categories.map(cat => (
            <option key={cat.id} value={cat.id}>{cat.name}</option>
          ))}
        </select>
      </div>

      {!initialValues && (
        <div className="p-4 bg-gray-50 rounded-lg border border-gray-200 space-y-4">
          <div className="flex items-center gap-3">
            <input 
              type="checkbox" 
              name="isRecurring" 
              checked={formData.isRecurring} 
              onChange={handleChange} 
              className="w-5 h-5 text-blue-600 rounded focus:ring-blue-500" 
            />
            <label className="text-gray-700 font-medium">Da li se ovaj događaj ponavlja svake nedelje?</label>
          </div>

          {formData.isRecurring && (
            <div>
              <label className={labelClass}>Broj nedelja (koliko ukupno ponavljanja)</label>
              <input 
                type="number" 
                name="numberOfWeeks" 
                min="1" 
                max="52" 
                value={formData.numberOfWeeks} 
                onChange={handleChange} 
                className={inputClass} 
                placeholder="Unesite broj nedelja..." 
              />
            </div>
          )}
        </div>
      )}

      <div className="flex items-center gap-3 p-4 bg-gray-50 rounded-lg border border-gray-200">
        <input type="checkbox" name="isPrivate" checked={formData.isPrivate} onChange={handleChange} className="w-5 h-5 text-blue-600 rounded focus:ring-blue-500" />
        <label className="text-gray-700 font-medium">Označi kao privatan događaj</label>
      </div>

      <button 
        type="submit" 
        disabled={isSubmitting}
        className="w-full bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 px-6 rounded-lg transition-all active:scale-[0.98] disabled:opacity-70"
      >
        {isSubmitting ? 'Čuvanje u toku...' : 'Sačuvaj događaj'}
      </button>
    </form>
  );
};

export default EventForm;