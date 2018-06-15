const path = require('path');
const webpack = require('webpack');

module.exports = {
    resolve: { extensions: ['.ts', '.js'] },
    devtool: 'inline-source-map',
    module: {
        rules: [{ test: /\.ts?$/, loader: 'ts-loader' }]
    },
  entry: {
    'blazor': './src/Boot.ts',
    'blazor.min': './src/Boot.ts',
  },
    output: { path: path.join(__dirname, '/dist'), filename: '[name].js' },
  plugins: [
    new webpack.optimize.UglifyJsPlugin({
      minimize: true,
      include: /\.min\.js$/,
      compress: {
        warnings: false
      },
    })
  ]
};
