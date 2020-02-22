#ifndef GENPROTO_TESTTYPES_H
#define GENPROTO_TESTTYPES_H
#include <QObject>
#include <QScopedPointer>
#include <QJSValue>
#include <QJsonValue>
namespace Tests {
class QTestTypesPrivate;
class QTestTypes : public QObject {
	Q_OBJECT
public:
	QTestTypes(QObject* parent = nullptr);
	~QTestTypes();
	Q_INVOKABLE void testParamDouble(double val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamFloat(float val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamInt(int val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamUInt(quint32 val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamLong(qint64 val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamULong(ulong val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamBool(bool val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamString(QJsonValue val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamByte(quint32 val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamBytes(QByteArray val, QJSValue state, QJSValue callback);
private slots:
	void testParamDoubleHandler(int requestId, QString error);
	void testParamFloatHandler(int requestId, QString error);
	void testParamIntHandler(int requestId, QString error);
	void testParamUIntHandler(int requestId, QString error);
	void testParamLongHandler(int requestId, QString error);
	void testParamULongHandler(int requestId, QString error);
	void testParamBoolHandler(int requestId, QString error);
	void testParamStringHandler(int requestId, QString error);
	void testParamByteHandler(int requestId, QString error);
	void testParamBytesHandler(int requestId, QString error);
private:
	QScopedPointer<QTestTypesPrivate> const d_priv;
};
}
#endif // GENPROTO_TESTTYPES_H
