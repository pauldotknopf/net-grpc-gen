#ifndef GENPROTO_TESTTYPES_H
#define GENPROTO_TESTTYPES_H
#include <QObject>
#include <QScopedPointer>
#include <QJSValue>
#include <QJsonValue>
#include <QVariant>
namespace Tests {
class QTestTypesPrivate;
class QTestTypes : public QObject {
	Q_OBJECT
public:
	QTestTypes(QObject* parent = nullptr);
	~QTestTypes();
	Q_INVOKABLE void testParamDouble(bool val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamFloat(bool val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamInt(bool val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamUInt(bool val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamLong(bool val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamULong(bool val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamBool(bool val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamString(QVariant val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamByte(bool val, QJSValue state, QJSValue callback);
	Q_INVOKABLE void testParamBytes(QByteArray val, QJSValue state, QJSValue callback);
signals:
	void testEvent(QVariant val);
private slots:
	void testParamDoubleHandler(bool result, int requestId, QString error);
	void testParamFloatHandler(bool result, int requestId, QString error);
	void testParamIntHandler(bool result, int requestId, QString error);
	void testParamUIntHandler(bool result, int requestId, QString error);
	void testParamLongHandler(bool result, int requestId, QString error);
	void testParamULongHandler(bool result, int requestId, QString error);
	void testParamBoolHandler(bool result, int requestId, QString error);
	void testParamStringHandler(QVariant result, int requestId, QString error);
	void testParamByteHandler(bool result, int requestId, QString error);
	void testParamBytesHandler(QByteArray result, int requestId, QString error);
private:
	QScopedPointer<QTestTypesPrivate> const d_priv;
};
}
#endif // GENPROTO_TESTTYPES_H
