const target = 'http://localhost:5292';

module.exports = {
  '/api': {
    target: target,
    secure: false,
    changeOrigin: true,
    pathRewrite: {
      '^/api': ''
    }
  }
};
