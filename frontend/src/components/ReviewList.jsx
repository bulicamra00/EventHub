import React from 'react';

const ReviewList = ({ reviews }) => {
  if (!reviews || reviews.length === 0) {
    return <p className="text-gray-500 italic">Nema ocena za ovaj događaj.</p>;
  }

  return (
    <div className="space-y-4">
      {reviews.map((review) => (
        <div key={review.id} className="bg-white p-4 rounded-lg border border-gray-100 shadow-sm">
          <div className="flex justify-between items-start mb-2">
            <div>
              <span className="font-bold text-gray-800 block">{review.userName}</span>
              <span className="text-xs text-gray-400">
                {new Date(review.createdAt).toLocaleDateString('sr-RS')}
              </span>
            </div>
            <div className="text-yellow-500 font-bold ml-4">
              {'★'.repeat(review.rating)}{'☆'.repeat(5 - review.rating)}
            </div>
          </div>
          <p className="text-gray-700 mt-2 text-sm">{review.comment}</p>
        </div>
      ))}
    </div>
  );
};

export default ReviewList;