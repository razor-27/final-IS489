import { useToast } from '../context/ToastContext';
import { getApiErrorMessage } from '../lib/utils';

export function useErrorHandler() {
  const toast = useToast();
  return function handleError(error: unknown) {
    toast.error(getApiErrorMessage(error));
  };
}
