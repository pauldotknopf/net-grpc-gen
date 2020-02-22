#ifndef GENPROTO_TESTTYPES_WORKER_H
#define GENPROTO_TESTTYPES_WORKER_H
#include <QObject>
#include <QScopedPointer>
#include <QJsonValue>
namespace Tests {
class QTestTypesWorkerPrivate;
class QTestTypesWorker : public QObject {
	Q_OBJECT
public:
	QTestTypesWorker();
	~QTestTypesWorker();
	void testParamDouble(double request, int requestId);
	void testParamFloat(float request, int requestId);
	void testParamInt(int request, int requestId);
	void testParamUInt(quint32 request, int requestId);
	void testParamLong(qint64 request, int requestId);
	void testParamULong(ulong request, int requestId);
	void testParamBool(bool request, int requestId);
	void testParamString(QJsonValue request, int requestId);
	void testParamByte(quint32 request, int requestId);
	void testParamBytes(QByteArray request, int requestId);
signals:
	void testParamDoubleDone(int requestId, QString error);
	void testParamFloatDone(int requestId, QString error);
	void testParamIntDone(int requestId, QString error);
	void testParamUIntDone(int requestId, QString error);
	void testParamLongDone(int requestId, QString error);
	void testParamULongDone(int requestId, QString error);
	void testParamBoolDone(int requestId, QString error);
	void testParamStringDone(int requestId, QString error);
	void testParamByteDone(int requestId, QString error);
	void testParamBytesDone(int requestId, QString error);
private:
	QScopedPointer<QTestTypesWorkerPrivate> const d_priv;
};
}
#endif // GENPROTO_TESTTYPES_WORKER_H
