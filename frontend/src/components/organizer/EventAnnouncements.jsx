import { useState } from 'react';
import toast from 'react-hot-toast';
import api from '../../api';

const EventAnnouncements = ({ eventId }) => {
  const [subject, setSubject] = useState('');
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSend = async (e) => {
    e.preventDefault();
    if (!subject.trim() || !message.trim()) {
      toast.error("Molimo popunite naslov i tekst obaveštenja.");
      return;
    }

    try {
      setLoading(true);
      const response = await api.post('/notifications/send', {
        eventId: eventId,
        subject: subject.trim(),
        message: message.trim()
      });

      toast.success(response.data.message || "Obaveštenje je uspešno poslato učesnicima!");
      setSubject('');
      setMessage('');
    } catch (error) {
      console.error("Greška pri slanju obaveštenja:", error);
      const errorMsg = error.response?.data?.message || "Slanje obaveštenja nije uspelo.";
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white border rounded-lg shadow-sm p-6 max-w-2xl mx-auto space-y-6">
      <div>
        <h3 className="text-lg font-semibold text-gray-800">Slanje obaveštenja učesnicima</h3>
        <p className="text-sm text-gray-500 mt-1">
          Pošaljite važnu poruku svim korisnicima koji imaju aktivnu kartu za ovaj događaj. Poruka će im stići kao email i in-app notifikacija.
        </p>
      </div>

      <form onSubmit={handleSend} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Naslov obaveštenja</label>
          <input
            type="text"
            value={subject}
            onChange={(e) => setSubject(e.target.value)}
            placeholder="Npr. Promena satnice ili važna napomena..."
            maxLength={150}
            className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Tekst poruke</label>
          <textarea
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            placeholder="Unesite detalje poruke za učesnike..."
            rows={5}
            className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-blue-600 text-white py-2 rounded-lg font-medium hover:bg-blue-700 transition disabled:opacity-50"
        >
          {loading ? "Slanje u toku..." : "Pošalji obaveštenje svima"}
        </button>
      </form>
    </div>
  );
};

export default EventAnnouncements;