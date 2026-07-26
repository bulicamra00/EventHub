import { useState } from 'react';
import api from '../api';
import toast from 'react-hot-toast';

const Review = ({ eventId, onReviewSubmitted, canReview, disabledMessage }) => {
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!canReview) return; 
    
    setSubmitting(true);
    try {
      await api.post('/Reviews/create', { eventId, rating, comment });
      toast.success("Hvala na oceni!");
      setComment("");
      if (onReviewSubmitted) onReviewSubmitted();
    } catch (err) {
      const message = err.response?.data?.message || "Greška pri slanju ocene.";
      toast.error(message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form 
      onSubmit={handleSubmit} 
      className={`bg-white p-6 rounded-lg border border-gray-200 shadow-sm mt-6 transition-opacity ${!canReview ? 'opacity-75' : 'opacity-100'}`}
    >
      <h3 className="font-bold text-lg mb-4">Ostavite utisak</h3>
      
      <div className="flex gap-2 mb-4">
        {[1, 2, 3, 4, 5].map((star) => (
          <button
            type="button"
            key={star}
            disabled={!canReview}
            onClick={() => setRating(star)}
            className={`text-2xl transition-colors ${!canReview ? 'cursor-not-allowed' : 'cursor-pointer'} ${rating >= star ? 'text-yellow-500' : 'text-gray-300'}`}
          >
            ★
          </button>
        ))}
      </div>

      <textarea
        value={comment}
        onChange={(e) => setComment(e.target.value)}
        className={`w-full p-3 border rounded-lg mb-4 ${!canReview ? 'bg-gray-100 cursor-not-allowed' : ''}`}
        placeholder={!canReview ? disabledMessage : "Podelite vaše iskustvo..."}
        rows="3"
        disabled={!canReview}
        required={canReview}
      />

      <button 
        type="submit" 
        disabled={submitting || !canReview}
        className={`w-full py-2 rounded-lg font-bold transition-colors ${
          !canReview 
            ? "bg-gray-300 text-gray-500 cursor-not-allowed" 
            : "bg-blue-600 text-white hover:bg-blue-700"
        }`}
      >
        {submitting ? "Slanje..." : !canReview ? "Nije moguće oceniti" : "Pošalji ocenu"}
      </button>

      {!canReview && disabledMessage && (
        <p className="text-sm text-red-600 mt-3 font-medium bg-red-50 p-2 rounded">
          {disabledMessage}
        </p>
      )}
    </form>
  );
};

export default Review;