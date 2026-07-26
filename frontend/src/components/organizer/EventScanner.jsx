import { useState } from 'react';
import toast from 'react-hot-toast';
import api from '../../api';

const EventScanner = ({ eventId }) => {
  const [ticketCode, setTicketCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [lastScanned, setLastScanned] = useState(null);

  const handleScan = async (e) => {
    e.preventDefault();
    if (!ticketCode.trim()) {
      toast.error("Unesite ili skenirajte kod karte.");
      return;
    }

    try {
      setLoading(true);
      const response = await api.post('/tickets/scan', {
        ticketCode: ticketCode.trim(),
        eventId: eventId
      });

      toast.success(response.data.message || "Karta je uspešno skenirana!");
      setLastScanned({
        code: ticketCode,
        time: new Date().toLocaleTimeString(),
        success: true
      });
      setTicketCode(''); 
    } catch (error) {
      console.error("Greška pri skeniranju:", error);
      const errorMsg = error.response?.data?.message || "Skeniranje nije uspelo: Karta je nevažeća, već iskorišćena, otkazana ili pripada drugom događaju.";
      toast.error(errorMsg);
      
      setLastScanned({
        code: ticketCode,
        time: new Date().toLocaleTimeString(),
        success: false,
        error: errorMsg
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white border rounded-lg shadow-sm p-6 max-w-xl mx-auto space-y-6">
      <div>
        <h3 className="text-lg font-semibold text-gray-800">Skeniranje karata na ulazu</h3>
        <p className="text-sm text-gray-500 mt-1">
          Unesite kod karte ručno ili putem skenera bar-koda/QR koda za ovaj događaj.
        </p>
      </div>

      <form onSubmit={handleScan} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Kod karte</label>
          <input
            type="text"
            value={ticketCode}
            onChange={(e) => setTicketCode(e.target.value)}
            placeholder="Unesite kod karte..."
            className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
            autoFocus
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-blue-600 text-white py-2 rounded-lg font-medium hover:bg-blue-700 transition disabled:opacity-50"
        >
          {loading ? "Proveravanje..." : "Validiraj i skeniraj kartu"}
        </button>
      </form>

      {lastScanned && (
        <div className={`p-4 rounded-lg border ${lastScanned.success ? 'bg-green-50 border-green-200' : 'bg-red-50 border-red-200'}`}>
          <h4 className={`font-semibold text-sm ${lastScanned.success ? 'text-green-800' : 'text-red-800'}`}>
            {lastScanned.success ? '✔ Uspešno skenirano' : '✖ Neuspešno skeniranje'}
          </h4>
          <p className="text-xs text-gray-600 mt-1">Kod: <span className="font-mono">{lastScanned.code}</span></p>
          <p className="text-xs text-gray-600">Vreme: {lastScanned.time}</p>
          {!lastScanned.success && (
            <p className="text-xs text-red-600 mt-1 font-medium">{lastScanned.error}</p>
          )}
        </div>
      )}
    </div>
  );
};

export default EventScanner;