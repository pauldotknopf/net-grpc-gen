#ifndef GENPROTO_TESTTYPES_WORKER_H
#define GENPROTO_TESTTYPES_WORKER_H
#include <QObject>
#include <QScopedPointer>
#include <QJsonValue>
#include <QVariant>
namespace Tests {
class QTestTypesWorkerPrivate;
class QTestTypesWorker : public QObject {
	Q_OBJECT
public:
	QTestTypesWorker();
	~QTestTypesWorker();
	void testParamDouble(bool request, int requestId);
	void testParamFloat(bool request, int requestId);
	void testParamInt(bool request, int requestId);
	void testParamUInt(bool request, int requestId);
	void testParamLong(bool request, int requestId);
	void testParamULong(bool request, int requestId);
	void testParamBool(bool request, int requestId);
	void testParamString(QVariant request, int requestId);
	void testParamByte(bool request, int requestId);
	void testParamBytes(QByteArray request, int requestId);
	void processEvent(void* event);
signals:
	void testParamDoubleDone(bool val, int requestId, QString error);
	void testParamFloatDone(bool val, int requestId, QString error);
	void testParamIntDone(bool val, int requestId, QString error);
	void testParamUIntDone(bool val, int requestId, QString error);
	void testParamLongDone(bool val, int requestId, QString error);
	void testParamULongDone(bool val, int requestId, QString error);
	void testParamBoolDone(bool val, int requestId, QString error);
	void testParamStringDone(QVariant val, int requestId, QString error);
	void testParamByteDone(bool val, int requestId, QString error);
	void testParamBytesDone(QByteArray val, int requestId, QString error);
	void testEventRaised(QVariant val);
private:
	QScopedPointer<QTestTypesWorkerPrivate> const d_priv;
};
}
#endif // GENPROTO_TESTTYPES_WORKER_H
