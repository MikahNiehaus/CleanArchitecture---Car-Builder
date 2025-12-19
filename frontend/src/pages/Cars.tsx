import React from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { carsApi } from '../services/api';
import type { Car } from '../types';

const Cars: React.FC = () => {
  const queryClient = useQueryClient();

  const { data: cars, isLoading, error } = useQuery({
    queryKey: ['cars'],
    queryFn: carsApi.getAll,
  });

  const deleteMutation = useMutation({
    mutationFn: carsApi.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cars'] });
    },
  });

  const handleDelete = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this car?')) {
      try {
        await deleteMutation.mutateAsync(id);
      } catch (error) {
        console.error('Failed to delete car:', error);
      }
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="text-gray-600">Loading cars...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="text-red-600">Error loading cars. Please try again.</div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="sm:flex sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Cars</h1>
          <p className="mt-2 text-sm text-gray-700">Manage your car inventory</p>
        </div>
        <div className="mt-4 sm:mt-0">
          <Link
            to="/cars/create"
            className="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            Add New Car
          </Link>
        </div>
      </div>

      {cars && cars.length === 0 ? (
        <div className="text-center mt-12">
          <p className="text-gray-500 text-lg">No cars found. Add your first car!</p>
        </div>
      ) : (
        <div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {cars?.map((car: Car) => (
            <div
              key={car.id}
              className="bg-white overflow-hidden shadow-md rounded-lg hover:shadow-lg transition-shadow"
            >
              <div className="px-6 py-4">
                <h3 className="text-xl font-semibold text-gray-900">
                  {car.make} {car.model}
                </h3>
                <p className="text-sm text-gray-600 mt-1">Year: {car.year}</p>
                <p className="text-lg font-bold text-blue-600 mt-2">
                  ${car.price.toLocaleString()}
                </p>
                {car.description && (
                  <p className="text-sm text-gray-700 mt-2 line-clamp-2">{car.description}</p>
                )}
              </div>
              <div className="px-6 py-4 bg-gray-50 border-t border-gray-200 flex justify-end space-x-2">
                <Link
                  to={`/cars/edit/${car.id}`}
                  className="px-3 py-1 text-sm font-medium text-blue-600 hover:text-blue-800"
                >
                  Edit
                </Link>
                <button
                  onClick={() => handleDelete(car.id)}
                  disabled={deleteMutation.isPending}
                  className="px-3 py-1 text-sm font-medium text-red-600 hover:text-red-800 disabled:opacity-50"
                >
                  {deleteMutation.isPending ? 'Deleting...' : 'Delete'}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Cars;
