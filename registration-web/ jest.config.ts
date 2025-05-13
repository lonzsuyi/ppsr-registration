export default {
    testEnvironment: 'jsdom',
    moduleNameMapper: {
      '\\.(css|scss|sass|less)$': 'identity-obj-proxy',
    },
    setupFilesAfterEnv: ['<rootDir>/jest.setup.ts'],
    transform: {
      '^.+\\.tsx?$': 'ts-jest',
    },
    testMatch: ['**/__tests__/**/*.(test|spec).ts?(x)'],
  };