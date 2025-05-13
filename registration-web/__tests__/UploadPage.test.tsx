import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import UploadPage from '../src/app/upload/file/page';

describe('UploadPage', () => {
  it('renders title', () => {
    render(<UploadPage />);
    expect(screen.getByText(/Upload CSV/i)).toBeInTheDocument();
  });
});